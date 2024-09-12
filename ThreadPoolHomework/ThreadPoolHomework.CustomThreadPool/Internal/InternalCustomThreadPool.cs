using System.Diagnostics;

namespace ThreadPoolHomework.CustomThreadPool.Internal;

internal class InternalCustomThreadPool : IDisposable
{
    private const int PoolTaskSpinTimeout = 10;

    private readonly Guid _guid = Guid.NewGuid();
    private readonly bool _memorizingOnly;


    private readonly Thread[] _threads;
    private readonly CancellationTokenSource _threadPoolCancellationTokenSource = new();

    private readonly Channel<InternalCustomTask> _customTaskChannel = new(89);
    private readonly HashSet<InternalCustomTask> _queuedTasks = new();
    private volatile bool _hasQueuedTasks;

    private const int UndefinedThreadsCount = -1;


    private void MasterThreadJob(CancellationToken cancellationToken)
    {
        var spinWait = new TwoStepSpinWait(PoolTaskSpinTimeout);
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_hasQueuedTasks)
                {
                    spinWait.SpinOnce();
                    continue;
                }

                lock (_queuedTasks)
                {
                    var readyToRunTasks = _queuedTasks.Where(x => x.ReadyToRun).ToArray();
                    foreach (var task in readyToRunTasks)
                    {
                        _customTaskChannel.Write(task);
                    }
                    _queuedTasks.ExceptWith(readyToRunTasks);

                    if (_queuedTasks.Count == 0)
                    {
                        _hasQueuedTasks = false;
                    }
                }

                spinWait.SpinOnce();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ThreadJob(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {

                cancellationToken.ThrowIfCancellationRequested();
                var task = _customTaskChannel.Read(cancellationToken);
                task.Running = true;
                task.Run();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void MergeThreadPools(InternalCustomTask task)
    {
        lock (_queuedTasks)
        {
            lock (task.ThreadPool._queuedTasks)
            {
                _queuedTasks.UnionWith(task.ThreadPool._queuedTasks);
                if (task.ThreadPool._queuedTasks.Count != 0)
                {
                    _hasQueuedTasks = true;
                }
                task.ThreadPool = this;
            }
        }
    }

    private void MemorizingEnqueueTask(InternalCustomTask task)
    {
        lock (_queuedTasks)
        {
            _queuedTasks.Add(task);
        }
    }

    public void Enqueue(InternalCustomTask task)
    {
        if (task.Parent != null && task.Parent.ThreadPool._guid != _guid )
        {
            throw new InvalidOperationException("Parent task not on current thread pool");
        }

        if (task.ThreadPool._guid != _guid)
        {
            if (task.ThreadPool._memorizingOnly)
            {
                MergeThreadPools(task);
            }
            else
            {
                throw new InvalidOperationException("Task already ran on different thread pool");
            }
        }
        else
        {
            if (_memorizingOnly)
            {
                MemorizingEnqueueTask(task);
            }
            else
            {
                throw new InvalidOperationException("Task already ran on thread pool");
            }
        }
    }

    private InternalCustomThreadPool(bool memorizingOnly, int threadPoolCapacity)
    {
        _memorizingOnly = memorizingOnly;
        if (memorizingOnly)
        {
            _threads = Array.Empty<Thread>();
        }
        else
        {
            var threadsCount = (threadPoolCapacity == UndefinedThreadsCount
                ? Process.GetCurrentProcess().Threads.Count
                : threadPoolCapacity) + 1;

            _threads = new Thread[threadsCount];


            _threads[0] = new Thread(() => MasterThreadJob(_threadPoolCancellationTokenSource.Token))
            {
                Priority = ThreadPriority.AboveNormal
            };
            _threads[0].Start();

            for (var i = 1; i < threadsCount; i++)
            {
                _threads[i] = new Thread(() => ThreadJob(_threadPoolCancellationTokenSource.Token));
                _threads[i].Start();
            }
        }
    }

    internal static InternalCustomThreadPool CreateMemorizingThreadPool()
    {
        return new InternalCustomThreadPool(true, UndefinedThreadsCount);
    }

    internal static InternalCustomThreadPool CreateExecutiveThreadPool()
    {
        return new InternalCustomThreadPool(false, UndefinedThreadsCount);
    }

    internal static InternalCustomThreadPool CreateExecutiveThreadPool(int threadPoolCapacity)
    {
        return new InternalCustomThreadPool(false, threadPoolCapacity);
    }

    public void Dispose()
    {
        _threadPoolCancellationTokenSource.Cancel();
        foreach (var thread in _threads)
        {
            thread.Join();
        }
        _customTaskChannel.Dispose();
    }
}
