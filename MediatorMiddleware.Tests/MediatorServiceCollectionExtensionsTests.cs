using MediatorMiddleware.Tests.Stubs;

namespace MediatorMiddleware.Tests;

public class MediatorServiceCollectionExtensionsTests
{
    private readonly Assembly _dynamicAssembly =
        DynamicAssemblyUtils.CompileSourceCode(
            DynamicCodeBuilder.StartWithUsings().WithNamespace().WithTodoItemQueryFeature().BuildCodeAsString());

    private Type ReqType
    {
        get { return _dynamicAssembly.GetTypes().Single(x => x.Name == "GetTodoQuery"); }
    }

    private Type ResponseType
    {
        get { return _dynamicAssembly.GetTypes().Single(x => x.Name == "TodoItemVm"); }
    }
    private Type HandlerType
    {
        get { return _dynamicAssembly.GetTypes().Single(x => x.Name == "GetTodoItemQueryHandler"); }
    }

    [Fact]
    public void AddMediator_AddsIMediatorServiceWithImplementation_Ok()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddMediatorMiddleware(options => options.RegisterServicesFromAssembly(_dynamicAssembly));
        
        // Assert
        var mediatorDescriptor = services
            .SingleOrDefault(x => x.ServiceType == typeof(IMediator));
        
        // Compare service descriptors 
        Assert.NotNull(mediatorDescriptor);
        Assert.Equal(typeof(IMediator), mediatorDescriptor.ServiceType);
        Assert.Equal(typeof(Mediator), mediatorDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, mediatorDescriptor.Lifetime);
    }
    
    [Fact]
    public void AddMediator_AddsISenderServiceWithImplementationFactory_Ok()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddMediatorMiddleware(options => options.RegisterServicesFromAssembly(_dynamicAssembly));
        var mediatorDescriptor = services
            .SingleOrDefault(x => x.ServiceType == typeof(ISender));
        
        // Assert
        Assert.NotNull(mediatorDescriptor);
        Assert.Equal(typeof(ISender), mediatorDescriptor.ServiceType);
        Assert.NotNull(mediatorDescriptor.ImplementationFactory);
        Assert.Equal(ServiceLifetime.Scoped, mediatorDescriptor.Lifetime);
    }

    [Fact]
    public void AddMediator_AddsIPublishServiceWithImplementationFactory_Ok()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddMediatorMiddleware(options => options.RegisterServicesFromAssembly(_dynamicAssembly));
        var mediatorDescriptor = services
            .SingleOrDefault(x => x.ServiceType == typeof(IPublisher));
        
        // Assert
        Assert.NotNull(mediatorDescriptor);
        Assert.Equal(typeof(IPublisher), mediatorDescriptor.ServiceType);
        Assert.NotNull(mediatorDescriptor.ImplementationFactory);
        Assert.Equal(ServiceLifetime.Scoped, mediatorDescriptor.Lifetime);
    }
    
    [Fact]
    public void AssemblyScanning_RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICallTracer, CallTracer>();
        services.AddMediatorMiddleware(options =>
        {
            options.RegisterServicesFromAssembly(_dynamicAssembly);
        });
        using var sp = services.BuildServiceProvider();
        
        
        
        var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(ReqType, ResponseType);
        
        var handler =sp.CreateScope().ServiceProvider.GetService(handlerInterface);
        
        Assert.NotNull(handler);
        Assert.IsType(HandlerType, handler);
    }
}