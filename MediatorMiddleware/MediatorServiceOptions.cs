using System.Reflection;

namespace MediatorMiddleware;

public class MediatorServiceOptions
{
    internal List<Assembly> AssembliesToRegister { get; } = [];
    internal List<Type> OpenBehaviorTypes { get; } = [];
    internal List<Type> OpenRequestPreProcessorTypes { get; } = [];
    
    //public ServiceLifetime Lifetime { get; private set; } = ServiceLifetime.Transient;

    #region Public Medods

    public MediatorServiceOptions RegisterServicesFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);
        return this;
    }
    
    public MediatorServiceOptions AddOpenBehavior(Type openBehaviorType)
    {
        if (!openBehaviorType.IsGenericTypeDefinition)
            throw new ArgumentException(
                $"Type {openBehaviorType.Name} must be an open generic type.",
                nameof(openBehaviorType));

        if (!ImplementsOpenGeneric(openBehaviorType, typeof(IPipelineBehavior<,>)))
            throw new ArgumentException(
                $"Type {openBehaviorType.Name} must implement IPipelineBehavior<,>.",
                nameof(openBehaviorType));

        OpenBehaviorTypes.Add(openBehaviorType);
        return this;
    }

    public MediatorServiceOptions AddOpenRequestPreProcessor(Type openPreProcessorType)
    {
        if (!openPreProcessorType.IsGenericTypeDefinition)
            throw new ArgumentException(
                $"Type {openPreProcessorType.Name} must be an open generic type.",
                nameof(openPreProcessorType));

        if (!ImplementsOpenGeneric(openPreProcessorType, typeof(IRequestPreProcessor<>)))
            throw new ArgumentException(
                $"Type {openPreProcessorType.Name} must implement IRequestPreProcessor<>.",
                nameof(openPreProcessorType));

        OpenRequestPreProcessorTypes.Add(openPreProcessorType);
        return this;
    }

    private static bool ImplementsOpenGeneric(Type type, Type openGenericInterface)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);
    }

    #endregion
}