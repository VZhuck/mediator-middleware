namespace MediatorMiddleware;

internal abstract class RequestHandlerWrapper
{
    public abstract Task<TResponse> Handle<TResponse>(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper
    where TRequest : IRequest<TResponse>
{
    public override Task<TResult> Handle<TResult>(IRequest<TResult> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return (Task<TResult>)(object)HandleInternal((TRequest)request, serviceProvider, cancellationToken);
    }

    private static Task<TResponse> HandleInternal(TRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>()
            ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}.");

        var preProcessors = serviceProvider.GetServices<IRequestPreProcessor<TRequest>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();

        RequestHandlerDelegate<TResponse> handlerDelegate = ct => handler.Handle(request, ct);

        // Wrap handler with pre-processors
        var innerDelegate = handlerDelegate;
        handlerDelegate = async ct =>
        {
            foreach (var preProcessor in preProcessors)
            {
                await preProcessor.Process(request, ct).ConfigureAwait(false);
            }

            return await innerDelegate(ct).ConfigureAwait(false);
        };

        // Wrap with pipeline behaviors (reverse order so first-registered runs outermost)
        foreach (var behavior in behaviors.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = ct => behavior.Handle(request, next, ct);
        }

        return handlerDelegate(cancellationToken);
    }
}
