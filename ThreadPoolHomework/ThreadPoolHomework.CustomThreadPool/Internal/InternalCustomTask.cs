using System.Diagnostics;

namespace ThreadPoolHomework.CustomThreadPool.Internal;


internal class InternalCustomTask
{

    private const int GetResultSpinTimeout = 10;

    private object? _result;
    private Exception? _thrown;
    private readonly Func<object> _evaluate;
    private readonly CancellationToken _cancellationToken;

    internal readonly InternalCustomTask? Parent;
    internal InternalCustomThreadPool ThreadPool;

    internal bool Running { get; set; }
    internal bool Completed { get; private set; }
    internal bool ReadyToRun => Parent == null || Parent.Completed;

    internal void Run()
    {
        Debug.Assert(!Completed);
        try
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var result = _evaluate();
            _result = result;
        }
        catch (Exception e)
        {
            _thrown = e;
        }
        finally
        {
            Completed = true;
        }
    }

    internal object GetResultBlocked()
    {
        var spinWait = new TwoStepSpinWait(GetResultSpinTimeout);

        while (!Completed)
        {
            if (!Running && _cancellationToken.IsCancellationRequested)
            {
                Completed = true;
                _thrown = new OperationCanceledException();
            }
            spinWait.SpinOnce();
        }

        if (_result != null)
            return _result;

        Debug.Assert(_thrown != null);
        throw new AggregateException(_thrown);
    }

    internal InternalCustomTask ContinueWith(Func<object, object> continuation)
    {
        var continuationTask = new InternalCustomTask(
            () => continuation(GetResultBlocked()),
            _cancellationToken,
            ThreadPool,
            this
        );
        return continuationTask;
    }

    private InternalCustomTask(
        Func<object> func,
        CancellationToken cancellationToken,
        InternalCustomThreadPool threadPool,
        InternalCustomTask? parent
    )
    {
        Parent = parent;
        _evaluate = func;
        _cancellationToken = cancellationToken;
        ThreadPool = threadPool;
        threadPool.Enqueue(this);
    }

    internal InternalCustomTask(Func<object> func): this(
        func,
        CancellationToken.None,
        InternalCustomThreadPool.CreateMemorizingThreadPool(),
        null
    ){}

    internal InternalCustomTask(Func<object> func, CancellationToken token): this(
        func,
        token,
        InternalCustomThreadPool.CreateMemorizingThreadPool(),
        null
    ) {}

}
