using System.Text;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;

namespace MattEland.ML.Interactive.Nodes;

public class PipelineNodeTreeParser
{
    public PipelineNode ParseTree(SweepablePipeline pipeline)
    {
        PipelineNode root = BuildNode(pipeline);

        return root;
    }
    
    private static PipelineNode BuildNode(SweepablePipeline pipeline)
    {
        List<PipelineNode> children = new();
        foreach ((var key, SweepableEstimator? estimator) in pipeline.Estimators)
        {
            children.Add(BuildNode(estimator));
        }

        return new ChainNode(pipeline, children);
    }
    
    private static PipelineNode BuildNode(SweepableEstimator estimator) 
        => new GenericSweepableEstimatorNode(estimator);
}