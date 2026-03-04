using System.Collections.Concurrent;

namespace MediatorMiddleware;

public class Mediator (IServiceProvider provider) :IMediator
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapper> RequestHandlerCache = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var wrapper = RequestHandlerCache.GetOrAdd(requestType, static type =>
        {
            var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(type, GetResponseType(type));
            return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        });

        return wrapper.Handle(request, provider, cancellationToken);
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var handlers = provider.GetServices<INotificationHandler<TNotification>>();

        foreach (var handler in handlers)
        {
            await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    private static Type GetResponseType(Type requestType)
    {
        foreach (var iface in requestType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IRequest<>))
            {
                return iface.GetGenericArguments()[0];
            }
        }

        throw new InvalidOperationException($"Type {requestType.Name} does not implement IRequest<TResponse>.");
    }
    
    
    // public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    // {
    //     var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
    //     dynamic handler = provider.GetRequiredService(handlerType);
    //     
    //     return handler.Handle((dynamic)request, cancellationToken);
    // }
}