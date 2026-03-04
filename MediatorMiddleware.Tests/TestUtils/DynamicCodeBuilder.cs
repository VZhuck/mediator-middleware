using System.Text;

namespace MediatorMiddleware.Tests.TestUtils;

public class DynamicCodeBuilder
{
    private readonly StringBuilder _codeLines = new StringBuilder();

    private DynamicCodeBuilder WithDefaultUsings()
    {
        _codeLines.Append(@"
            using MediatorMiddleware.Abstractions;
            using System.Threading;
            using System.Threading.Tasks;
        ");

        return this;
    }
    
    public DynamicCodeBuilder WithNamespace(string @namespace = "DymamicCode")
    {
        _codeLines.Append($"namespace {@namespace};");
        _codeLines.AppendLine();
        return this;
    } 
    
    public DynamicCodeBuilder WithTodoItemQueryFeature(string @namespace = "DymamicCode")
    {
        _codeLines.Append(@"
            public record  GetTodoQuery: IRequest<TodoItemVm>
            {
                public int Id { get; set; }
            }

            public class TodoItemVm
            {
                public int Id { get; init; }
                
                public string? Title { get; init; }

                public bool Done { get; init; }

                public int Priority { get; init; }

                public string? Note { get; init; }
            }

            public class GetTodoItemQueryHandler : IRequestHandler<GetTodoQuery, TodoItemVm>
            {
                public Task<TodoItemVm> Handle(GetTodoQuery request, CancellationToken cancellationToken)
                {
                    return Task.FromResult(new TodoItemVm
                        {
                            Id = request.Id,
                            Done = true,
                            Note =  ""Hello World 100"",
                            Title = ""Note 100"",
                            Priority = 1
                        }
                    );
                }
            }
        ");
        
        return this;
    }

    public string BuildCodeAsString()
    {
        return _codeLines.ToString();
    }

    public static DynamicCodeBuilder StartWithUsings()
    {
        var builder = new DynamicCodeBuilder();
        
        return builder.WithDefaultUsings();
    } 
}