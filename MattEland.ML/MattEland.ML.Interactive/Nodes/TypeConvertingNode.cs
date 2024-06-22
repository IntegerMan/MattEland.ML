using MattEland.ML.Interactive.Details;
using Microsoft.ML.Transforms;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Nodes;

public class TypeConvertingNode : PipelineNode
{
    public TypeConvertingNode(TypeConvertingTransformer tct) : base(tct)
    {
    }

    public override string Note
    {
        get
        {
            string json = ReflectAsJson("_columns");
            List<TypeConvertDetails> columns = JsonConvert.DeserializeObject<List<TypeConvertDetails>>(json) ?? [];
            
            return $"Convert {string.Join(", ", columns.Select(c => c.ToString()))}";
        }
    }
}