namespace ThreadPoolHomework.CustomThreadPool.Internal;

internal class TwoStepSpinWait
{
    private SpinWait _spinner;
    private readonly int _timeout;

    public TwoStepSpinWait(int timeout)
    {
        _timeout = timeout;
    }

    public void SpinOnce()
    {
        if (!_spinner.NextSpinWillYield)
        {
            _spinner.SpinOnce();
        }
        else
        {
            Thread.Sleep(_timeout);
        }
    }
}
