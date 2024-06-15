using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.AspNetCore.Html;
using Microsoft.Data.Analysis;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace MattEland.ML.Interactive;

public class VisualizeTransformerExtension
{
    public static Task LoadAsync(Kernel kernel)
    {
        var mermaidKernel = kernel
            .FindKernels(k => k.KernelInfo.LanguageName == "Mermaid")
            .FirstOrDefault();
        
        if (mermaidKernel is null)
        {
            throw new KernelException($"{nameof(VisualizeTransformerExtension)} requires a kernel that supports Mermaid language");
        }

        kernel.VisitSubkernelsAndSelf(k =>
        {
            if (k is CSharpKernel csharpKernel)
            {
                var variableNameArg = new Argument<string>("variable-name", "The name of the variable to replace")
                    .AddCompletions(ctx => csharpKernel.ScriptState
                        .Variables
                        .Where(v => v.Value is ITransformer or TransformerChain<ITransformer>)
                        .Select(v => v.Name));
                
                var command = new Command("#!transformer", "Visualizes a transformer or transformer chain")
                {
                    variableNameArg
                };
                csharpKernel.AddDirective(command);
                command.Handler = CommandHandler.Create(Visualize);

                var command2 = new Command("#!transformer-test", "Visualizes a transformer or transformer chain");
                csharpKernel.AddDirective(command2);
                command2.Handler = CommandHandler.Create(VisualizeTest);
                
                async Task Visualize(
                    string variableName,
                    KernelInvocationContext context)
                {
                    if (csharpKernel.TryGetValue(variableName, out ITransformer transformer))
                    {
                        string markdown = transformer.ToMermaid();

                        await context.HandlingKernel.RootKernel.SendAsync(new SubmitCode(markdown,
                            targetKernelName: mermaidKernel.Name));
                    }
                    else
                    {
                        Console.Error.WriteLine($"{variableName} is not an ITransformer");
                    }
                }
                
                async Task VisualizeTest(KernelInvocationContext context)
                {
                    string markdown = "stateDiagram-v2\n  [*] --> TransformerChain`1_1\n  state TransformerChain`1_1 {\n    [*] --> MissingValueReplacingTransformer1_1\n    MissingValueReplacingTransformer1_1 --> TypeConvertingTransformer1_2\n    TypeConvertingTransformer1_2 --> Transformer1_3\n    Transformer1_3 --> ColumnConcatenatingTransformer1_4\n    ColumnConcatenatingTransformer1_4 --> BinaryPredictionTransformer`11_5\n    BinaryPredictionTransformer`11_5 --> [*]\n  }\n  TransformerChain`1_1 --> [*]";

                    await context.HandlingKernel.RootKernel.SendAsync(new SubmitCode(markdown,
                        targetKernelName: mermaidKernel.Name));
                }
            }
        });

        Formatter.Register<ITransformer>(t => t.ToMermaid(), mimeType: "text/vnd.mermaid");

        return Task.CompletedTask;
    }
}