using JetBrains.Annotations;

namespace MediatorMiddleware.Tests.Stubs;

public sealed record OrderPlaced(string OrderId) : INotification;

public sealed class OrderPlacedHandler1(ICallTracer tracer) : INotificationHandler<OrderPlaced>
{
    public Task Handle(OrderPlaced notification, CancellationToken cancellationToken)
    {
        tracer.TraceCall($"Handler1:{notification.OrderId}");
        return Task.CompletedTask;
    }
}


[UsedImplicitly]
public sealed class OrderPlacedHandler2(ICallTracer tracer) : INotificationHandler<OrderPlaced>
{
    public Task Handle(OrderPlaced notification, CancellationToken cancellationToken)
    {
        tracer.TraceCall($"Handler2:{notification.OrderId}");
        return Task.CompletedTask;
    }
}