namespace MediatorMiddleware.Tests.TestUtils;

public interface ICallTracer
{
    IList<string> CallTraces { get; }
    
    void TraceCall(string trace);
}