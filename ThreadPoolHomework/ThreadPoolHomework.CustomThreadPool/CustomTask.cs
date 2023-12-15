using ThreadPoolHomework.CustomThreadPool.Internal;

namespace ThreadPoolHomework.CustomThreadPool;

public class CustomTask<TResult>
{

    internal readonly Internal.InternalCustomTask InternalCustomTask;

    private CustomTask(Internal.InternalCustomTask internalCustomTask)
    {
        InternalCustomTask = internalCustomTask;
    }

    public CustomTask(Func<TResult> eval)
    {
        InternalCustomTask = new InternalCustomTask(() => eval());
    }


    public CustomTask(Func<TResult> eval, CancellationToken cancellationToken)
    {
        InternalCustomTask = new InternalCustomTask(() => eval(), cancellationToken);
    }

    public bool Completed => InternalCustomTask.Completed;
    public TResult Result => (TResult)InternalCustomTask.GetResultBlocked();
    public CustomTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
    {
        var childTask = InternalCustomTask.ContinueWith(result => continuation((TResult)result));
        return new CustomTask<TNewResult>(childTask);
    }
}
