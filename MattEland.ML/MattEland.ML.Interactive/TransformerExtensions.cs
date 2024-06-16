using System.Collections;
using System.Reflection;
using System.Text;
using MattEland.ML.Interactive.Details;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive;

public static class TransformerExtensions
{
    public static string ToMermaid(this ITransformer transformer)
    {
        StringBuilder sb = new();
        sb.AppendLine("stateDiagram-v2");

        int index = 1;
        sb.AppendLine($"{GetName(transformer)}_{index} : " + GetDisplayName(transformer));
        if (transformer is TransformerChain<ITransformer> chain)
        {
            sb.AppendLine(chain.BuildFlowchartFromTransformerChain($"{index}"));
        }
        else
        {
            sb.AppendLine(GetName(transformer));
        }

        return sb.ToString();
    }

    private static string GetDisplayName(ITransformer transformer)
    {
        if (transformer.GetType().FullName.Contains("TextFeaturizingEstimator"))
            return "TextFeaturizingEstimator_Transformer";
        
        return new string(transformer.GetType().Name.Where(char.IsLetter).ToArray());
    }

    private static string BuildFlowchartFromTransformerChain(this TransformerChain<ITransformer> transformerChain, string prefix = "", string name = "")
    {
        List<ITransformer> chain = transformerChain.ToList();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            name = GetName(transformerChain);
        }

        StringBuilder sb = new();
        sb.AppendLine($"state {name}_{prefix} {{");

        // Define fields
        int index = 1;
        foreach (var transformer in chain)
        {
            string subprefix = prefix + "_" + index++;
            string elName = GetName(transformer) + subprefix;
            sb.AppendLine(elName + ": " + GetDisplayName(transformer));
        }

        index = 1;
        foreach (var transformer in chain)
        {
            string subprefix = prefix + "_" + index;
            if (transformer is TransformerChain<ITransformer> subChain)
            {
                sb.AppendLine(BuildFlowchartFromTransformerChain(subChain, subprefix));
            }
            else if (transformer.GetType().FullName!.Contains("TextFeaturizingEstimator")) // This relies on the private TextFeaturizingEstimator.Transformer class which is private
            {
                FieldInfo chainField = transformer.GetType().GetField("_chain", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
                sb.AppendLine(BuildFlowchartFromTransformerChain((TransformerChain<ITransformer>)chainField!.GetValue(transformer)!, subprefix, name: "TextFeaturizingEstimator_Transformer"));
            }

/*
            if (!transformer.GetType().Name.Contains("Prediction"))
            {
                AddNotes(transformer, sb, elName);
            }
        */

            //last = node;
        }

        // Render relationships
        for (int i = 0; i < chain.Count - 1; i++)
        {
            string a = GetName(chain[i]) + prefix + "_" + (i + 1);
            string b = GetName(chain[i + 1]) + prefix + "_" + (i + 2);

            sb.AppendLine(a + " --> " + b);
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void AddNotes(ITransformer transformer, StringBuilder sb, string elName)
    {
        IEnumerable<FieldInfo> fields = transformer.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
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

        if (fieldInfo.Any())
        {
            sb.AppendLine("note right of " + elName);
            
            sb.AppendLine(BuildNote(transformer, fieldInfo));

            sb.AppendLine("end note");
        }
    }

    private static string BuildNote(ITransformer transformer, Dictionary<string, string> fieldInfo)
    {
        StringBuilder sb = new();

        switch (transformer)
        {
            case TypeConvertingTransformer:
                TypeConvertDetails.BuildNote(fieldInfo, sb);
                break;
            case ColumnConcatenatingTransformer:
                ColumnConcatDetails.BuildNote(fieldInfo, sb);
                break;
            case MissingValueReplacingTransformer:
            {
                string[] values = fieldInfo["_repValues"].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                string[] colPairs = fieldInfo["ColumnPairs"].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                int index = 0;
                foreach (var col in colPairs)
                {
                    sb.AppendLine($"Replace missing values in {col} with {values[index]}");
                }
                break;
            }
            default:
            {
                foreach (var kvp in fieldInfo)
                {
                    sb.AppendLine(kvp.Key + ": " + kvp.Value);
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

        if (value is ValueTuple<string, string>[] tuples)
        {
            return string.Join(',', tuples.Select(t => t.Item1 == t.Item2 ? t.Item1 : $"({t.Item1}, {t.Item2})"));
        }

        if (value.GetType().FullName.Contains("ColumnOptions"))
        {
            return "ColumnOptions";
        }

        if (value is object[] objArr)
        {
            return string.Join(", ", objArr);
        }

        if (value is IEnumerable objEnumerable)
        {
            return string.Join(", ", objEnumerable);
        }

        if (value is IEnumerable<object> objEnum)
        {
            return string.Join(", ", objEnum);
        }


        return value.ToString();
    }

    private static string GetName(ITransformer transformer)
    {
        if (transformer.GetType().FullName.Contains("TextFeaturizingEstimator"))
        {
            return "TextFeaturizingEstimator_Transformer";
        }
        
        string name = transformer.GetType().Name!
            .Replace("`", "")
            .Replace("+", "_");
        return name;
    }
}