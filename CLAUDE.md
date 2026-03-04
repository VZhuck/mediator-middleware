# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
Custom Mediator pattern library for .NET (similar to MediatR). Provides request/response dispatching, notification pub/sub, and pipeline behaviors for cross-cutting concerns.

### Project Structure

```
MediatorMiddleware.sln                          # Solution file
├── Directory.Build.props                       # Shared build settings (TreatWarningsAsErrors, nullable, etc.)
├── Directory.Packages.props                    # Central package version management
│
├── MediatorMiddleware.Abstractions/            # Public interfaces and contracts (no dependencies)
│   ├── IMediator.cs                            # Combined ISender + IPublisher interface
│   ├── INotification.cs                        # Marker interface for notification messages
│   ├── INotificationHandler.cs                 # Notification handler contract
│   ├── IPipelineBehavior.cs                    # Middleware pipeline behavior contract
│   ├── IPublisher.cs                           # Notification publishing contract
│   ├── IRequest.cs                             # Marker interface for request messages
│   ├── IRequestHandler.cs                      # Request handler contract
│   ├── IRequestPreProcessor.cs                 # Pre-processing hook before handler execution
│   └── ISender.cs                              # Request dispatching contract
│
├── MediatorMiddleware/                         # Mediator implementation
│   └── ...                                     # other implementation types
│
└── MediatorMiddleware.Tests/                   # Unit tests (xUnit)
    └── ...                                     
```

## Build & Test Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~MediatorTests.Send_ShouldReturnResponse"

# Run tests in a specific project
dotnet test MediatorMiddleware.Tests/
```

## Build Configuration

- **Target framework**: .NET 10.0 (all projects via `Directory.Build.props`)
- **TreatWarningsAsErrors**: enabled globally
- **Nullable reference types**: enabled globally
- **Central package management**: versions in `Directory.Packages.props`

## Architecture Notes

- `Mediator.Send<TResponse>()` resolves handlers at runtime via `typeof(IRequestHandler<,>).MakeGenericType()` + `dynamic` dispatch
- Handler registration scans assemblies for types implementing `IRequestHandler<,>` and registers them as scoped services
- `IMediator` is registered scoped; `ISender` and `IPublisher` are registered as factories resolving `IMediator`
- Tests use `DynamicAssemblyUtils` (Roslyn compilation) and `DynamicCodeBuilder` (fluent code-gen) to create in-memory assemblies for testing assembly scanning without pre-compiled test projects

## Not Yet Implemented

`Publish()` (notifications), pipeline behaviors, pre-processors, configurable service lifetimes