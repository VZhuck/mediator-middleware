namespace MediatorMiddleware.Tests.Stubs;

// Request has no handler
public sealed record UnhandledRequest : IRequest<string>;