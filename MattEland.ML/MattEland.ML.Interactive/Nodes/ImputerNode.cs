using System.Text;
using Microsoft.ML.Transforms;

namespace MattEland.ML.Interactive.Nodes;

public class ImputerNode : PipelineNode
{
    public ImputerNode(MissingValueReplacingTransformer mvr) : base(mvr)
    {
    }

    public override string Note
    {
        get
        {
            string[] values = ReflectAsString("_repValues").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new();
            foreach (var groupedValue in values.GroupBy(v => v))
            {
                sb.AppendLine($"Replace missing values in {groupedValue.Count()} column(s) with {groupedValue.Key}");
            }

            return sb.ToString();
        }
    }
}