using System.Text;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace MattEland.ML.Interactive;

public static class TransformerExtensions
{
    public static string ToMermaid(this ITransformer transformer)
    {
        StringBuilder sb = new();
        sb.AppendLine("stateDiagram-v2");

        int index = 1;
        sb.AppendLine(Indent(1) + "[*] --> " + transformer.GetType().Name + "_" + index);
        if (transformer is TransformerChain<ITransformer> chain)
        {
            sb.AppendLine(Indent(1) + chain.BuildFlowchartFromTransformerChain($"{index}", 1));
        }
        else
        {
            sb.AppendLine(Indent(1) + "[*] --> " + transformer.GetType().Name + " --> [*]");
        }
        sb.AppendLine(Indent(1) + transformer.GetType().Name + "_" + index + " --> [*]");
        
        return sb.ToString();
    }

    private static string BuildFlowchartFromTransformerChain(this TransformerChain<ITransformer> chain, string prefix = "", int indentLevel = 1)
    {
        string name = chain.GetType().Name;
        StringBuilder sb = new();
        sb.AppendLine($"state {name}_{prefix} {{");
        string last = "[*]";
        int index = 1;
        indentLevel++;
        foreach (var transformer in chain)
        {
            string subprefix = prefix + "_" + index;

            string node;
            
            if (transformer is TransformerChain<ITransformer> subChain)
            {
                node = BuildFlowchartFromTransformerChain(subChain, subprefix, indentLevel + 1);
            }
            else
            {
                node = transformer.GetType().Name + subprefix;
            }

            sb.AppendLine(Indent(indentLevel) + last + " --> " + node);
            last = node;
            index++;
        }

        sb.AppendLine(Indent(indentLevel) + last + " --> [*]");
        sb.AppendLine(Indent(--indentLevel) + "}");
        return sb.ToString();
    }

    private static string Indent(int indentLevel)
    {
        string indent = "";
        for (int i = 0; i < indentLevel; i++)
        {
            indent += "  ";
        }

        return indent;
    }
}