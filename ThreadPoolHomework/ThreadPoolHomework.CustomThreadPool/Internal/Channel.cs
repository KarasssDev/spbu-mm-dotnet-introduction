using System.Collections.Concurrent;

namespace ThreadPoolHomework.CustomThreadPool.Internal;

internal class Channel<T>: IDisposable
{
    private readonly ConcurrentQueue<T> _queue = new();
    private readonly Semaphore _semaphore = new(0, 1);
    private readonly int _waitTimeout;

    public Channel(int waitTimeout)
    {
        _waitTimeout = waitTimeout;
    }

    public void Write(T data)
    {
        _queue.Enqueue(data);
        _semaphore.Release();
    }

    public T Read(CancellationToken token)
    {
        while (true)
        {
            token.ThrowIfCancellationRequested();
            if (_semaphore.WaitOne(_waitTimeout))
            {
                var dequeued = _queue.TryDequeue(out var data);
                if (dequeued && data != null)
                {
                    return data;
                }
                throw new Exception("Read from empty channel");
            }
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
