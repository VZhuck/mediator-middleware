namespace MediatorMiddleware.Tests.TestUtils;

public class CallTracer: ICallTracer
{
    public IList<string> CallTraces { get; } = new List<string>();

    public void TraceCall(string trace) => CallTraces.Add(trace);
}