using MediatorMiddleware.Tests.Stubs;

namespace MediatorMiddleware.Tests;

public class MediatorTests
{
    private static ServiceProvider BuildServiceProvider(Action<MediatorServiceOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddScoped<ICallTracer, CallTracer>();
        services.AddMediatorMiddleware(configure ?? (options => { }));
        return services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Send_DispatchesToCorrectHandler_Ok()
    {
        await using var sp = BuildServiceProvider(opts =>
            opts.RegisterServicesFromAssembly(typeof(PingHandler).Assembly));

        var mediator = sp.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        var result = await mediator.Send(new Ping(10, "Hello"));

        Assert.Equal( 10, result.RequestId);
        Assert.Equal( "Pong: Hello", result.RequestMsg);
    }

    [Fact]
    public async Task Send_WhenNoHandlerRegistered_Throws()
    {
        // Arrange
        await using var sp = BuildServiceProvider();
        var mediator = sp.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
        
        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(new UnhandledRequest()));
    }
    
    [Fact]
    public async Task Send_RegisteredLoginBehavior_Applied()
    {
        await using var sp = BuildServiceProvider(opts =>
        {
            opts.RegisterServicesFromAssembly(typeof(PingHandler).Assembly);
            opts.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        var provider = sp.CreateScope().ServiceProvider;
        var mediator = provider.GetRequiredService<IMediator>();
        var tracer = provider.GetRequiredService<ICallTracer>();
        
        await mediator.Send(new Ping(100, "Behavior"));

        Assert.Equal(tracer.CallTraces,
            ["LoggingBefore:Ping", "PingHandler: requestId:100", "LoggingAfter:Ping"]);
    }

    [Fact]
    public async Task PipelineBehaviors_ExecuteInOrder()
    {
        await using var sp = BuildServiceProvider(opts =>
        {
            opts.RegisterServicesFromAssembly(typeof(PingHandler).Assembly);
            opts.AddOpenBehavior(typeof(LoggingBehavior<,>));
            opts.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        var provider = sp.CreateScope().ServiceProvider;
        var mediator = provider.GetRequiredService<IMediator>();
        var tracer = provider.GetRequiredService<ICallTracer>();
        
        await mediator.Send(new Ping(100, "Test"));

        Assert.Equal(
            tracer.CallTraces,
            [
                "LoggingBefore:Ping",
                "ValidationBefore:Ping",
                "PingHandler: requestId:100",
                "ValidationAfter:Ping",
                "LoggingAfter:Ping"
            ]);
    }

    [Fact]
    public async Task PreProcessors_RunBeforeHandler()
    {
        await using var sp = BuildServiceProvider(opts =>
        {
            opts.RegisterServicesFromAssembly(typeof(PingHandler).Assembly);
            opts.AddOpenRequestPreProcessor(typeof(TestPreProcessor<>));
        });

        var provider = sp.CreateScope().ServiceProvider;
        var mediator = provider.GetRequiredService<IMediator>();
        var tracer = provider.GetRequiredService<ICallTracer>();
        
        await mediator.Send(new Ping(1000,"Hello"));
        
        Assert.Equal(tracer.CallTraces,
            new List<string>
            {
                "TestPreProcessor:Ping",
                "PingHandler: requestId:1000"
            });
    }

    [Fact]
    public async Task Publish_FansOutToAllHandlers()
    {
        await using var sp = BuildServiceProvider(opts =>
            opts.RegisterServicesFromAssembly(typeof(OrderPlacedHandler1).Assembly));

        var provider = sp.CreateScope().ServiceProvider;
        var mediator = provider.GetRequiredService<IMediator>();
        var tracer = provider.GetRequiredService<ICallTracer>();
        
        await mediator.Publish(new OrderPlaced("ORD-1"));

        Assert.Equivalent(tracer.CallTraces.ToArray(), new[] { "Handler1:ORD-1", "Handler2:ORD-1" });
    }

    [Fact]
    public async Task Publish_WithNoHandlers_NoException()
    {
        await using var sp = BuildServiceProvider();
        var mediator = sp.CreateScope().ServiceProvider.GetRequiredService<IMediator>();

        // Should not throw
        await mediator.Publish(new UnhandledNotification());
    }
}