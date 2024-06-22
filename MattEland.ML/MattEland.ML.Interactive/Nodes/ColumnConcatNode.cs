using MattEland.ML.Interactive.Details;
using Microsoft.ML.Data;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Nodes;

public class ColumnConcatNode : PipelineNode
{
    public ColumnConcatNode(ColumnConcatenatingTransformer concat) : base(concat)
    {
    }

    public override string Note
    {
        get
        {
            string json = ReflectAsJson("_columns");
            List<ColumnConcatDetails> details = JsonConvert.DeserializeObject<List<ColumnConcatDetails>>(json) ?? new();
            return string.Join(", ", details.Select(d => d.ToString()));
        }
    }
}