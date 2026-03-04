namespace MediatorMiddleware.Tests.Stubs;

public sealed class LoggingBehavior<TRequest, TResponse> (ICallTracer tracer) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        tracer.TraceCall($"LoggingBefore:{typeof(TRequest).Name}");
        var response = await next(cancellationToken);
        tracer.TraceCall($"LoggingAfter:{typeof(TRequest).Name}");
        return response;
    }
}