using System.Collections;
using System.Reflection;
using System.Text;
using MattEland.ML.Interactive.Details;
using MattEland.ML.Interactive.Nodes;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive;

public static class PipelineExtensions
{
    public static string ToMermaid(this SweepablePipeline transformer, bool annotate = false, int maxDepth = 3)
    {
        StringBuilder sb = new();
        sb.AppendLine("stateDiagram-v2");
        int index = 1;

        PipelineNodeTreeParser parser = new();

        PipelineNode root = parser.ParseTree(transformer);

        if (root.HasChildren)
        {
            List<PipelineNode> children = root.Children.ToList();
            sb.AppendLine(BuildFlowchartFromChildren(root, children, index.ToString(), isLR: false, annotate: annotate, maxDepth: maxDepth));
        }
        else
        {
            sb.AppendLine($"t{index}: {GetDisplayName(root)}");
        }
        
        return sb.ToString();
    }

    private static string BuildFlowchartFromChildren(this PipelineNode parent, List<PipelineNode> children, string prefix = "", bool isLR = false, bool annotate = false, int maxDepth = 0)
    {
        StringBuilder sb = new();
        sb.AppendLine($"state \"{GetDisplayName(parent)}\" AS t{prefix} {{");

        if (isLR)
        {
            sb.AppendLine("direction LR");
        }

        // Loop over all children
        int index = 1;
        foreach (var transformer in children)
        {
            // Define the node
            string id = $"{prefix}_{index++}";
            sb.AppendLine($"t{id}: {GetDisplayName(transformer)}");

            // Loop over any nested children
            List<PipelineNode> subTransformers = transformer.Children.ToList();
            bool renderedChildren = false;
            if (maxDepth > 1 && subTransformers.Any())
            {
                renderedChildren = true;
                sb.AppendLine(BuildFlowchartFromChildren(
                    transformer,
                    subTransformers,
                    prefix: id, 
                    isLR: !isLR,
                    maxDepth: maxDepth - 1,
                    annotate: annotate));
            }

            // If we're annotating, it's time to do that. Don't show notes for expanded parents, though.
            if (annotate && !renderedChildren)
            {
                AddNote(sb, $"t{id}", isLR, transformer.Note);
            }
        }

        // Render relationships
        for (int i = 0; i < children.Count - 1; i++)
        {
            string a = $"t{prefix}_{i + 1}";
            string b = $"t{prefix}_{i + 2}";

            sb.AppendLine($"{a} --> {b}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
    
    private static string GetDisplayName(PipelineNode node) => node.Name.Replace("<","_").Replace(">","_");

    private static void AddNote(StringBuilder sb, string elName, bool isLR, string? note)
    {
        if (string.IsNullOrWhiteSpace(note)) return;
        
        if (isLR)
        {
            sb.AppendLine($"note left of {elName}");
        }
        else
        {
            sb.AppendLine($"note right of {elName}");
        }

        // Break the note into lines at the word level. Ensure no line is longer than 80 characters.
        StringBuilder line = new();
        foreach (var word in note.Split(' '))
        {
            if (line.Length + word.Length > 80)
            {
                sb.AppendLine(line.ToString());
                line.Clear();
            }

            line.Append(word);
            line.Append(' ');
        }
        sb.AppendLine(line.ToString().Trim());
        sb.AppendLine("end note");
    }
}