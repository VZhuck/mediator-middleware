using System.Reflection;

namespace MediatorMiddleware;

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddMediatorMiddleware(
        this IServiceCollection services, Action<MediatorServiceOptions> configure)
    {
        var options = new MediatorServiceOptions();
        configure(options);
        
        services.AddMediatorRequiredServices();
        
        var assemblies = options.AssembliesToRegister.Any()
            ? options.AssembliesToRegister
            : [Assembly.GetCallingAssembly()];
        foreach (var assembly in assemblies)
        {
            AddAssemblyRequestHandlers(services, assembly);
        }
        
        foreach (var openBehaviorType in options.OpenBehaviorTypes)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), openBehaviorType);
        }

        foreach (var openPreProcessorType in options.OpenRequestPreProcessorTypes)
        {
            services.AddTransient(typeof(IRequestPreProcessor<>), openPreProcessorType);
        }
        
        return services;
    }
    
    [Obsolete("Use AddMediatorMiddleware instead") ]
    public static IServiceCollection AddMediator(this IServiceCollection services, MediatorServiceOptions options)
    {
        services.AddMediatorRequiredServices();

        var assemblies = options.AssembliesToRegister.Any()
            ? options.AssembliesToRegister
            : [Assembly.GetCallingAssembly()];
        foreach (var assembly in assemblies)
        {
            AddAssemblyRequestHandlers(services, assembly);
        }

        return services;
    }

    #region Private Methods
    
    private static void AddAssemblyRequestHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaceType = typeof(IRequestHandler<,>);
        var notificationInterfaceType = typeof(INotificationHandler<>);

        var handlerTypes = assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(type =>
                type.GetInterfaces().Where(i => i.IsGenericType
                                                && (i.GetGenericTypeDefinition() == handlerInterfaceType ||
                                                    i.GetGenericTypeDefinition() == notificationInterfaceType))
                    .Select(i => new { InterfaceType = i, ImplementationType = type }));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType.InterfaceType, handlerType.ImplementationType);
        }
    }

    private static IServiceCollection AddMediatorRequiredServices(this IServiceCollection services)
    {
        // TODO: Consider Configurable Lifetime
        // services.TryAdd(new ServiceDescriptor(typeof(ISender), sp => sp.GetRequiredService<IMediator>(), options.Lifetime));
        services.AddScoped<IMediator, MediatorMiddleware.Mediator>();
        services.AddScoped<ISender>(sp => sp.GetRequiredService<IMediator>());
        services.AddScoped<IPublisher>(sp => sp.GetRequiredService<IMediator>());
        return services;
    }
    
    #endregion Private Methods
}