using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MediatorMiddleware.Tests.TestUtils;

public static class DynamicAssemblyUtils
{
    public static Assembly CompileSourceCode(string sourceCode)
    {
        // Define the syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Define assembly references (essential for the compiled code to work)
        // This example includes common .NET references. You may need more depending on your code.
        MetadataReference[] references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            // MetadataReference.CreateFromFile(typeof(CancellationToken).Assembly.Location),
            // MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IRequestHandler<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Mediator).Assembly.Location),

            MetadataReference.CreateFromFile(
                Path.Combine(
                    Path.GetDirectoryName(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location)!,
                    "System.Runtime.dll"))
        ];

        var assemblyName = $"DynamicAssembly_{Guid.NewGuid().ToString("N").Substring(0, 8)}.dll";
        
        // Configure compilation settings
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        // Emit the assembly into a MemoryStream
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            // Handle compilation errors
            Console.WriteLine("Compilation errors:");
            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (var diagnostic in failures)
            {
                Console.Error.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            }

            throw new Exception($"Compilation errors: {assemblyName} wasn't successfully compiled. ");
        }

        // Load the assembly from the MemoryStream
        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
}