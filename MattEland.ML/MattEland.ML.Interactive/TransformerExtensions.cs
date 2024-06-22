using System.Collections;
using System.Reflection;
using System.Text;
using MattEland.ML.Interactive.Details;
using MattEland.ML.Interactive.Nodes;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive;

public static class TransformerExtensions
{
    public static string ToMermaid(this ITransformer transformer, bool annotate = false, int maxDepth = 3)
    {
        StringBuilder sb = new();
        sb.AppendLine("stateDiagram-v2");
        int index = 1;

        TransformerNodeTreeParser parser = new();

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
        
        /*
        List<ITransformer> children = GetChildren(transformer).ToList();
        if (children.Any())
        {
            sb.AppendLine(BuildFlowchartFromChildren(transformer, children, $"{index}", isLR: false, annotate: annotate, maxDepth: maxDepth));
        }
        else
        {
            sb.AppendLine($"t{index}: {GetDisplayName(transformer)}");
        }
        */
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
    
    private static IEnumerable<ITransformer> GetChildren(ITransformer transformer)
    {
        FieldInfo? chainField = transformer.GetType().GetField("_chain", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        IEnumerable<ITransformer>? children;
        if (chainField != null)
        {
            children = chainField.GetValue(transformer) as IEnumerable<ITransformer>;
        }
        else if (transformer is TransformerChain<ITransformer> chain)
        {
            children = chain.ToList();
        }
        else if (transformer is OneHotEncodingTransformer encoder)
        {
            ITransformer? child = encoder.GetReflectedValue<ITransformer>("_transformer");
            children = child != null ? GetChildren(child) : null;
        } 
        else
        {
            children = transformer as IEnumerable<ITransformer>;
        }

        if (children != null)
        {
            foreach (var child in children)
            {
                yield return child;
            }
        }
    }

    private static string GetDisplayName(ITransformer transformer) => transformer.GetType().GetShortTypeName().Replace("<","_").Replace(">","_");
    
    private static string GetDisplayName(PipelineNode node) => node.Name.Replace("<","_").Replace(">","_");

    private static string BuildFlowchartFromChildren(this ITransformer parent, List<ITransformer> children, string prefix = "", bool isLR = false, bool annotate = false, int maxDepth = 0)
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
            List<ITransformer> subTransformers = GetChildren(transformer).ToList();
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
                AddNotes(transformer, sb, $"t{id}", isLR);
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

    private static void AddNotes(ITransformer transformer, StringBuilder sb, string elName, bool isLR)
    {
        IEnumerable<FieldInfo> fields = transformer.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => !f.Name.Contains("TrainSchema"))
            .Where(f => !f.FieldType.Name.Contains("TransformerChain"))
            .Where(f => !f.FieldType.Name.Contains("BitArray"))
            .Where(f => !f.FieldType.Name.Contains("BindableMapper"))
            .Where(f => !f.FieldType.Name.Contains("IHost"));

        Dictionary<string, string> fieldInfo = new();

        foreach (var field in fields)
        {
            string fieldName = GetFieldName(field).Replace("`", "");
            var objValue = field.GetValue(transformer);

            string value = field.FieldType.Name.Contains("ColumnOptions")
                ? JsonConvert.SerializeObject(objValue, Formatting.Indented)
                : GetFieldValue(objValue).Replace("`", "");

            if (!string.IsNullOrWhiteSpace(value))
            {
                fieldInfo[fieldName] = value;
            }
        }

        string note = BuildNote(transformer, fieldInfo);
        AddNote(sb, elName, isLR, note);
    }

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

    private static string BuildNote(ITransformer transformer, Dictionary<string, string> fieldInfo)
    {
        // Parents should just list children
        List<ITransformer> children = GetChildren(transformer).ToList();
        switch (children.Count)
        {
            case 1:
                return "1 Child Transformer: " + GetDisplayName(children.First());
            case > 1:
                return $"{children.Count} Child Transformers";
        }
        
        // Not a parent node, so let's get some details
        StringBuilder sb = new();
        switch (transformer) // TODO: This should switch to have a parser in front of it and shallow classes capable of handling either estimators or transformers
        {
            case TypeConvertingTransformer:
                TypeConvertDetails.BuildNote(fieldInfo, sb);
                break;
            case ColumnConcatenatingTransformer:
                ColumnConcatDetails.BuildNote(fieldInfo, sb);
                break;
            case MissingValueReplacingTransformer:
                MissingValuesDetails.BuildNote(fieldInfo, sb);
                break;
            case TextNormalizingTransformer:
                TextNormalizingDetails.BuildNote(fieldInfo, sb);
                break;
            case WordTokenizingTransformer:
                WordTokenizingDetails.BuildNote(fieldInfo, sb);
                break;
            case TokenizingByCharactersTransformer:
                TokenizingByCharactersDetails.BuildNote(fieldInfo, sb);
                break;
            case ColumnSelectingTransformer:
                ColumnSelectingDetails.BuildNote(fieldInfo, sb);
                break;
            case LpNormNormalizingTransformer:
                LpNormNormalizingDetails.BuildNote(fieldInfo, sb);
                break;
            case NgramExtractingTransformer:
                NgramExtractingDetails.BuildNote(fieldInfo, sb);
                break;
            case ValueToKeyMappingTransformer:
                ValueToKeyMappingDetails.BuildNote(fieldInfo, sb);
                break;
            case KeyToVectorMappingTransformer:
                KeyToVectorMappingDetails.BuildNote(fieldInfo, sb);
                break;
            default:
            {
                foreach (var kvp in fieldInfo)
                {
                    sb.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
                break;
            }
        }

        return sb.ToString();
    }

    private static string GetFieldName(MemberInfo field)
        => field.Name;

    private static string GetFieldValue(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string s)
        {
            return s;
        }

        if (value is ValueTuple<string, string>[] tuples)
        {
            return string.Join(',', tuples.Select(t => t.Item1 == t.Item2 ? t.Item1 : $"({t.Item1}, {t.Item2})"));
        }

        if (value is IEnumerable enumerable)
        {
            List<string> strs = new();
            foreach (var obj in enumerable)
            {
                strs.Add(GetFieldValue(obj));
            }

            return string.Join(',', strs);
        }

        if (value is object[] objArr)
        {
            return string.Join(", ", objArr);
        }

        if (value is IEnumerable<object> objEnum)
        {
            return string.Join(", ", objEnum);
        }

        return JsonConvert.SerializeObject(value);
        //return value.ToString() ?? "";
    }
}