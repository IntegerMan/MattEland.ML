using System.Reflection;
using Microsoft.ML.AutoML;

namespace MattEland.ML.Interactive.Nodes;

public class GenericSweepableEstimatorNode : PipelineNode
{
    public GenericSweepableEstimatorNode(SweepableEstimator est) : base(est)
    {
        // Reflect the internal FunctionName property getter to get the function name
        FieldInfo? funcNameField = SourceType.GetField("FunctionName", BindingFlags.Instance | BindingFlags.NonPublic);
        
        Note = funcNameField?.GetValue(est)?.ToString();
    }

    //public override string Name => _funcName ?? base.Name;
    public override string? Note { get; }
}