namespace MediatorMiddleware.Tests.Stubs;

public sealed record Ping(int RequestId, string RequestMsg) : IRequest<PingResponse>;

public class PingResponse(int requestId, string responseMsg)
{
    public int RequestId { get; } = requestId;
    public string RequestMsg { get; } = responseMsg;
}

public sealed class PingHandler (ICallTracer tracer) : IRequestHandler<Ping, PingResponse>
{
    public Task<PingResponse> Handle(Ping request, CancellationToken cancellationToken)
    {
        tracer.TraceCall($"PingHandler: requestId:{request.RequestId}");
        return Task.FromResult(new PingResponse(request.RequestId, $"Pong: {request.RequestMsg}"));
    }
}