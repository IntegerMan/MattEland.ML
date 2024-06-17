using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.ML;
using Microsoft.ML.Data;

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
            throw new KernelException(
                $"{nameof(VisualizeTransformerExtension)} requires a kernel that supports Mermaid language");
        }

        kernel.VisitSubkernelsAndSelf(k =>
        {
            if (k is CSharpKernel csharpKernel)
            {
                Argument<string> variableNameArg = new Argument<string>("variable-name", "The name of the variable to replace")
                    .AddCompletions(ctx => csharpKernel.ScriptState
                        .Variables
                        .Where(v => v.Value is ITransformer or TransformerChain<ITransformer>)
                        .Select(v => v.Name));
                
                var maxDepthOption = new Option<int>(new[] { "-d", "--depth" }, () => 3,
                    "The maximum depth");      
                
                var annotateOption = new Option<bool>(new[] { "-n", "--notes" }, () => false,
                    "Whether or not behavior notes will be added to elements on the diagram");

                Command vizCommand = new("#!transformer-vis", "Visualizes a transformer or transformer chain")
                {
                    variableNameArg,
                    maxDepthOption,
                    annotateOption
                };
                vizCommand.SetHandler(Visualize, variableNameArg, maxDepthOption, annotateOption);
                csharpKernel.AddDirective(vizCommand);
                
                KernelInvocationContext.Current?.Display(
                    new HtmlString(@"<details><summary>transformer-vis</summary>
    <p>This extension generates Flowcharts from ITransformers using the Mermaid kernel.</p>
    </details>"),
                    "text/html");

                async Task Visualize(
                    string variableName,
                    int maxDepth,
                    bool annotate)
                {
                    if (csharpKernel.TryGetValue(variableName, out ITransformer transformer))
                    {
                        string markdown = transformer.ToMermaid(annotate, maxDepth);

                        await KernelInvocationContext.Current.HandlingKernel.RootKernel.SendAsync(new SubmitCode(markdown,
                            targetKernelName: mermaidKernel.Name));
                    }
                    else
                    {
                        await Console.Error.WriteLineAsync($"{variableName} is not an ITransformer");
                    }
                }
                
                
            }
        });

        return Task.CompletedTask;
    }
}