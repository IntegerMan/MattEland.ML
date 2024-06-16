using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace MattEland.ML.Interactive;

public static class VisualizeTransformerExtension
{
    public static Task Load(Kernel kernel)
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
                KernelInvocationContext.Current?.Display(
                    new HtmlString(@"<details><summary>Transformer</summary>
    <p>This extension generates Flowcharts from ITransformers using the Mermaid kernel.</p>
    </details>"),
                    "text/html");

                var command2 = new Command("#!transformer-test", "Visualizes a transformer or transformer chain");
                csharpKernel.AddDirective(command2);
                command2.Handler = CommandHandler.Create(VisualizeTest);
                KernelInvocationContext.Current?.Display(
                    new HtmlString(@"<details><summary>Transformer-Test</summary>
    <p>This extension generates a sample Flowchart using the Mermaid kernel.</p>
    </details>"),
                    "text/html");
                
                async Task Visualize(
                    string variableName,
                    KernelInvocationContext context)
                {
                    if (csharpKernel.TryGetValue(variableName, out ITransformer transformer))
                    {
                        string markdown = transformer.ToMermaid();

                        KernelInvocationContext.Current?.Display(new HtmlString(markdown), "text/html");
                        
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
                    string markdown = "stateDiagram-v2\n  [*] --> TransformerChain1_1\n  state TransformerChain1_1 {\n    [*] --> MissingValueReplacingTransformer1_1\n    MissingValueReplacingTransformer1_1 --> TypeConvertingTransformer1_2\n    TypeConvertingTransformer1_2 --> Transformer1_3\n    Transformer1_3 --> ColumnConcatenatingTransformer1_4\n    ColumnConcatenatingTransformer1_4 --> BinaryPredictionTransformer11_5\n    BinaryPredictionTransformer11_5 --> [*]\n  }\n  TransformerChain1_1 --> [*]";

                    
                    await context.HandlingKernel.RootKernel.SendAsync(new SubmitCode(markdown,
                        targetKernelName: mermaidKernel.Name));
                }
            }
        });

        //Formatter.Register<ITransformer>(t => t.ToMermaid(), mimeType: "text/vnd.mermaid");

        // Finally, display some information to the user so they can see how to use the extension.
        PocketView view = div(
            code(nameof(VisualizeTransformerExtension)),
            " is loaded. It adds visualizations for ",
            code(typeof(ITransformer))
        );
        
        KernelInvocationContext.Current?.Display(view);
        return Task.CompletedTask;
    }
}