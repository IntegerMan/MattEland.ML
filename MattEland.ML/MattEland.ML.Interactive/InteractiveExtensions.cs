using System.CommandLine;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

namespace MattEland.ML.Interactive;

public static class InteractiveExtensions
{
    public static Task Load(Kernel kernel)
    {
        var mermaidKernel = kernel
            .FindKernels(k => k.KernelInfo.LanguageName == "Mermaid")
            .FirstOrDefault();

        kernel.VisitSubkernelsAndSelf(k =>
        {
            SetupReflection(k);
            SetupTransformerVisualizer(k, mermaidKernel);
            SetupPipelineVisualizer(k, mermaidKernel);
        });

        return Task.CompletedTask;
    }

    private static void SetupReflection(Kernel k)
    {
        if (k is not CSharpKernel csharpKernel) return;
        
        Argument<string> variableNameArg = new Argument<string>("variable-name", "The name of the variable to reflect")
            .AddCompletions(_ => csharpKernel.ScriptState.Variables.Select(v => v.Name));
                
        Command vizCommand = new("#!reflect", "Reflects the internal structure of the object")
        {
            variableNameArg
        };
        vizCommand.SetHandler(Visualize, variableNameArg);
        csharpKernel.AddDirective(vizCommand);
                
        KernelInvocationContext.Current?.Display(
            new HtmlString(@"<details><summary>reflect</summary>
    <p>This extension generates a list of properties on the object in tabular format.</p>
    </details>"),
            "text/html");

        async Task Visualize(string variableName)
        {
            if (csharpKernel.TryGetValue(variableName, out object obj))
            {
                StringBuilder sb = new();
                sb.AppendLine($"<h3>{obj.GetType().GetShortTypeName().HtmlEncode()}</h3>");
                sb.AppendLine("<table><thead><tr><th>Property</th><th>Type</th><th>Value</th></thead><tbody>");
                
                IEnumerable<FieldInfo> fields = obj.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    sb.AppendLine($"<tr><th>{field.Name}</th><td>{field.FieldType.GetShortTypeName().HtmlEncode()}</td><td>{field.GetValue(obj)}</td></tr>");
                }

                sb.AppendLine("</tbody></table>");
                KernelInvocationContext.Current?.Display(new HtmlString(sb.ToString()), "text/html");
            }
            else
            {
                await Console.Error.WriteLineAsync($"{variableName} is could not be read");
            }
        }
    }
    
    private static void SetupTransformerVisualizer(Kernel k, Kernel? mermaidKernel)
    {
        if (k is CSharpKernel csharpKernel && mermaidKernel != null)
        {
            Argument<string> variableNameArg = new Argument<string>("variable-name", "The name of the variable to analyze")
                .AddCompletions(ctx => csharpKernel.ScriptState
                    .Variables
                    .Where(v => v.Value is ITransformer or TransformerChain<ITransformer>)
                    .Select(v => v.Name));
                
            var maxDepthOption = new Option<int>(new[] { "-d", "--depth" }, () => 3,
                "The maximum depth");      
                
            var annotateOption = new Option<bool>(new[] { "-n", "--notes" }, () => false,
                "Whether or not behavior notes will be added to elements on the diagram");

            var mermaidOption = new Option<bool>(new[] { "-m", "--mermaid" }, () => false,
                "Whether or not the raw mermaid syntax is outputted");
            
            Command vizCommand = new("#!transformer-vis", "Visualizes a transformer or transformer chain")
            {
                variableNameArg,
                maxDepthOption,
                annotateOption,
                mermaidOption
            };
            vizCommand.SetHandler(Visualize, variableNameArg, maxDepthOption, annotateOption, mermaidOption);
            csharpKernel.AddDirective(vizCommand);
                
            KernelInvocationContext.Current?.Display(
                new HtmlString(@"<details><summary>transformer-vis</summary>
    <p>This extension generates Flowcharts from ITransformers using the Mermaid kernel.</p>
    </details>"),
                "text/html");

            async Task Visualize(
                string variableName,
                int maxDepth,
                bool annotate,
                bool showMarkdown)
            {
                if (csharpKernel.TryGetValue(variableName, out ITransformer transformer))
                {
                    string markdown = transformer.ToMermaid(annotate, maxDepth);

                    if (showMarkdown)
                    {
                        Console.WriteLine(markdown);
                    }

                    await KernelInvocationContext.Current.HandlingKernel.RootKernel.SendAsync(new SubmitCode(markdown,
                        targetKernelName: mermaidKernel.Name));
                }
                else
                {
                    await Console.Error.WriteLineAsync($"{variableName} is not an ITransformer");
                }
            }
        }
    }
    
    private static void SetupPipelineVisualizer(Kernel k, Kernel? mermaidKernel)
    {
        if (k is CSharpKernel csharpKernel && mermaidKernel != null)
        {
            Argument<string> variableNameArg = new Argument<string>("variable-name", "The name of the variable to analyze")
                .AddCompletions(ctx => csharpKernel.ScriptState
                    .Variables
                    .Where(v => v.Value is SweepablePipeline)
                    .Select(v => v.Name));
                
            var maxDepthOption = new Option<int>(new[] { "-d", "--depth" }, () => 3,
                "The maximum depth");      
                
            var annotateOption = new Option<bool>(new[] { "-n", "--notes" }, () => false,
                "Whether or not behavior notes will be added to elements on the diagram");

            var mermaidOption = new Option<bool>(new[] { "-m", "--mermaid" }, () => false,
                "Whether or not the raw mermaid syntax is outputted");
            
            Command vizCommand = new("#!pipeline-vis", "Visualizes a pipeline")
            {
                variableNameArg,
                maxDepthOption,
                annotateOption,
                mermaidOption
            };
            vizCommand.SetHandler(Visualize, variableNameArg, maxDepthOption, annotateOption, mermaidOption);
            csharpKernel.AddDirective(vizCommand);
                
            KernelInvocationContext.Current?.Display(
                new HtmlString(@"<details><summary>pipeline-vis</summary>
    <p>This extension generates Flowcharts from Pipelines using the Mermaid kernel.</p>
    </details>"),
                "text/html");

            async Task Visualize(
                string variableName,
                int maxDepth,
                bool annotate,
                bool showMarkdown)
            {
                if (csharpKernel.TryGetValue(variableName, out SweepablePipeline pipeline))
                {
                    string markdown = pipeline.ToMermaid(annotate, maxDepth);

                    if (showMarkdown)
                    {
                        Console.WriteLine(markdown);
                    }

                    await KernelInvocationContext.Current.HandlingKernel.RootKernel.SendAsync(new SubmitCode(markdown,
                        targetKernelName: mermaidKernel.Name));
                }
                else
                {
                    await Console.Error.WriteLineAsync($"{variableName} is not a Pipeline");
                }
            }
        }
    }
}