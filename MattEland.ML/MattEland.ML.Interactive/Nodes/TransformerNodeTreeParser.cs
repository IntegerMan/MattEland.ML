using System.Text;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;

namespace MattEland.ML.Interactive.Nodes;

public class TransformerNodeTreeParser
{
    public PipelineNode ParseTree(ITransformer transformer)
    {
        PipelineNode root = BuildNode(transformer);

        return root;
    }

    private static PipelineNode BuildNode(ITransformer transformer)
    {
        // Reflect the _chain field to get the transformers in the chain
        var innerChain = transformer.GetReflectedValue<TransformerChain<ITransformer>>("_chain");
        if (innerChain != null)
        {
            return new ChainNode(transformer, innerChain.Select(BuildNode));
        }

        return transformer switch
        {
            IEnumerable<ITransformer> chain => new ChainNode(transformer, chain.Select(BuildNode)),
            MissingValueReplacingTransformer mvr => new ImputerNode(mvr),
            TypeConvertingTransformer tct => new TypeConvertingNode(tct),
            ColumnConcatenatingTransformer concat => new ColumnConcatNode(concat),
            _ => new GenericNode(transformer)
        };
    }
}