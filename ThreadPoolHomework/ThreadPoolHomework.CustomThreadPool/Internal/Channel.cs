using System.Collections.Concurrent;

namespace ThreadPoolHomework.CustomThreadPool.Internal;

internal class Channel<T>
{
    private const int ReadSpinTimeout = 10;
    private readonly ConcurrentQueue<T> _queue = new();
    private readonly int _spinTimeout;

    public Channel(int spinTimeout)
    {
        _spinTimeout = spinTimeout;
    }

    public void Write(T data)
    {
        _queue.Enqueue(data);
    }

    public T Read(CancellationToken token)
    {
        var spinWait = new TwoStepSpinWait(ReadSpinTimeout);
        while (true)
        {
            token.ThrowIfCancellationRequested();
            if (_queue.IsEmpty)
            {
                spinWait.SpinOnce();
            }
            else
            {
                var dequeued = _queue.TryDequeue(out var data);
                if (dequeued && data != null)
                {
                    return data;
                }
            }
        }
    }
}
