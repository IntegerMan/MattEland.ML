using System.Reflection;

namespace MattEland.ML.Interactive.Nodes;

public class GenericNode(object node) : PipelineNode(node)
{
    public override string? Note
    {
        get
        {
            FieldInfo? summary = SourceType.GetField("Summary", BindingFlags.Static | BindingFlags.NonPublic);
            return summary?.GetValue(Source)?.ToString();
        }
    }
}