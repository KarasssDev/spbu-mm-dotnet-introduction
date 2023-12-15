namespace ThreadPoolHomework.CustomThreadPool;

public class CustomThreadPool: IDisposable
{
    private readonly Internal.InternalCustomThreadPool _threadPool;

    public CustomThreadPool(int capacity)
    {
        _threadPool = Internal.InternalCustomThreadPool.CreateExecutiveThreadPool(capacity);
    }

    public CustomThreadPool()
    {
        _threadPool = Internal.InternalCustomThreadPool.CreateExecutiveThreadPool();
    }

    public void Enqueue<TResult>(CustomTask<TResult> task)
    {
        _threadPool.Enqueue(task.InternalCustomTask);
    }

    public void Dispose()
    {
        _threadPool.Dispose();
    }
}
