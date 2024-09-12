using NUnit.Framework;
using ThreadPoolHomework.CustomThreadPool;

namespace ThreadPoolHomework.Tests;

public class CustomThreadPoolTests
{

    private const int ThreadsCount = 4;
    private CustomThreadPool.CustomThreadPool _threadPool;

    [SetUp]
    protected void SetUp()
    {
        _threadPool = new CustomThreadPool.CustomThreadPool(ThreadsCount);
    }

    [TearDown]
    public void TearDown()
    {
        _threadPool.Dispose();
    }

    private static void AssertThrowsAggregateExceptionWith<TException>(Action action)
    {
        try
        {
            action();
        }
        catch (AggregateException e)
        {
            Assert.That(e.InnerException is TException);
        }
    }

    [Test]
    public void ThreadPoolHasMultipleThreads()
    {
        var sleepTime = 200;
        var tasks = new List<CustomTask<int>>();
        var cancellationTokenSource = new CancellationTokenSource();

        for (int i = 0; i < ThreadsCount; i++)
        {
            tasks.Add(new CustomTask<int>(() =>
            {
                Thread.Sleep(sleepTime);
                return 4;
            }, cancellationTokenSource.Token));
        }

        foreach (var task in tasks)
        {
            _threadPool.Enqueue(task);
        }

        cancellationTokenSource.CancelAfter(sleepTime);
        foreach (var task in tasks)
        {
            Assert.That(task.Result, Is.EqualTo(4));
        }
    }


    [Test]
    public void EnqueueContinuationTaskThrown()
    {
        var task = new CustomTask<int>(() => 4);
        var childTask = task.ContinueWith(x => x + 4);

        Assert.Throws<InvalidOperationException>(() =>
        {
            _threadPool.Enqueue(childTask);
        });
    }

    [Test]
    public void EnqueueEnqueuedTaskThrown()
    {
        var task = new CustomTask<int>(() => 4);

        _threadPool.Enqueue(task);
        Assert.Throws<InvalidOperationException>(() =>
        {
            _threadPool.Enqueue(task);
        });
    }

    [Test]
    public void SingleTaskOnThreadPoolFinished()
    {
        var task = new CustomTask<int>(() => 4);

        _threadPool.Enqueue(task);

        Assert.That(task.Result, Is.EqualTo(4));
    }

    [Test]
    public void SingleThrownTaskOnThreadPoolFinished()
    {
        var task = new CustomTask<int>(() => throw new Exception());

        _threadPool.Enqueue(task);

        AssertThrowsAggregateExceptionWith<Exception>(() =>
        {
            var _ = task.Result;
        });
    }

    [Test]
    public void SingleTaskNotOnThreadPoolNotFinished()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var task = new CustomTask<int>(() => 4, cancellationTokenSource.Token);

        cancellationTokenSource.CancelAfter(100);

        AssertThrowsAggregateExceptionWith<OperationCanceledException>(() =>
        {
            var _ = task.Result;
        });
    }

    [Test]
    public void SingleThrownTaskNotOnThreadPoolNotFinished()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var task = new CustomTask<int>(() => throw new Exception(), cancellationTokenSource.Token);

        cancellationTokenSource.CancelAfter(100);

        AssertThrowsAggregateExceptionWith<OperationCanceledException>(() =>
        {
            var _ = task.Result;
        });
    }

    [Test]
    public void SingleContinuationTaskOnThreadPoolFinished()
    {

        var task = new CustomTask<int>(() => 4);
        var childTask = task.ContinueWith(x => x + 4);

        _threadPool.Enqueue(task);

        Assert.That(childTask.Result, Is.EqualTo(8));
    }

    [Test]
    public void SingleContinuationTaskNotOnThreadPoolNotFinished()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var task = new CustomTask<int>(() => 4, cancellationTokenSource.Token);
        var childTask = task.ContinueWith(x => x + 4);

        cancellationTokenSource.CancelAfter(100);

        AssertThrowsAggregateExceptionWith<OperationCanceledException>(() =>
        {
            var _ = childTask.Result;
        });
    }

    [Test]
    public void MultipleContinuationTaskOnThreadPoolFinished()
    {
        var task = new CustomTask<int>(() => 4);
        var childTask1 = task.ContinueWith(x => x + 4);
        var childTask2 = task.ContinueWith(x => x + 4);

        _threadPool.Enqueue(task);
        Assert.Multiple(() =>
        {
            Assert.That(task.Result, Is.EqualTo(4));
            Assert.That(childTask1.Result, Is.EqualTo(8));
            Assert.That(childTask2.Result, Is.EqualTo(8));
        });
    }

    [Test]
    public void TreeContinuationTaskOnThreadPoolFinished()
    {
        var task = new CustomTask<int>(() => 4);
        var childTask1 = task.ContinueWith(x => x + 4);
        var childTask2 = task.ContinueWith(x => x + 4);
        var childTask11 = childTask1.ContinueWith(x => x + 4);
        var childTask12 = childTask1.ContinueWith(x => x + 4);
        var childTask21 = childTask2.ContinueWith(x => x + 4);
        var childTask22 = childTask2.ContinueWith(x => x + 4);

        _threadPool.Enqueue(task);

        Assert.Multiple(() =>
        {
            Assert.That(task.Result, Is.EqualTo(4));
            Assert.That(childTask1.Result, Is.EqualTo(8));
            Assert.That(childTask2.Result, Is.EqualTo(8));
            Assert.That(childTask11.Result, Is.EqualTo(12));
            Assert.That(childTask12.Result, Is.EqualTo(12));
            Assert.That(childTask21.Result, Is.EqualTo(12));
            Assert.That(childTask22.Result, Is.EqualTo(12));
        });
    }
}
