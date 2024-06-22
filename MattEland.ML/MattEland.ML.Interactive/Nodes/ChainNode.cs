namespace MattEland.ML.Interactive.Nodes;

public class ChainNode(object node, IEnumerable<PipelineNode> children) : PipelineNode(node)
{
    private readonly List<PipelineNode> _nodes = children.ToList();

    public override bool HasChildren => _nodes.Count > 0;
    public override IEnumerable<PipelineNode> Children => _nodes;

    public override string Note 
        => _nodes.Count switch
        {
            1 => $"1 Child: {_nodes.First()}",
            _ => $"{_nodes.Count} Children"
        };

    public override string Name
    {
        get
        {
            // Gets the name of the SourceType type, excluding any generic annotation marking
            string name = SourceType.Name;
            int index = name.IndexOf('`');
            return index > 0 ? name[..index] : name;            
        }
    }
}