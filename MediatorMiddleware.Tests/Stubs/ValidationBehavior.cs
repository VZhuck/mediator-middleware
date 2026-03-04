namespace MediatorMiddleware.Tests.Stubs;

public sealed class ValidationBehavior<TRequest, TResponse> (ICallTracer tracer) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        tracer.TraceCall($"ValidationBefore:{typeof(TRequest).Name}");
        var response = await next(cancellationToken);
        tracer.TraceCall($"ValidationAfter:{typeof(TRequest).Name}");
        return response;
    }
}