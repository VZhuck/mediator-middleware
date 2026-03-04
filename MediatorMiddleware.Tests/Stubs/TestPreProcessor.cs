namespace MediatorMiddleware.Tests.Stubs;

public sealed class TestPreProcessor<TRequest>(ICallTracer tracer) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        tracer.TraceCall($"TestPreProcessor:{typeof(TRequest).Name}");
        return Task.CompletedTask;
    }
}