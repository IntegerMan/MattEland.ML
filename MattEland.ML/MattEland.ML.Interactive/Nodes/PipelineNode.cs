using System.Collections;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Nodes;

public abstract class PipelineNode
{
    protected PipelineNode(object obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        
        Source = obj;
    }

    public object Source { get; set; }

    public virtual string Name => SourceType.GetShortTypeName();

    public Type SourceType => Source.GetType();

    public virtual IEnumerable<PipelineNode> Children => Enumerable.Empty<PipelineNode>();
    
    public virtual string? Note { get; }
    public virtual bool HasChildren => Children.Any();

    public override string ToString() => Name + (string.IsNullOrWhiteSpace(Note) ? "" : $": {Note}");

    protected T? ReflectField<T>(string fieldName, object obj) 
        => obj.GetReflectedValue<T>(fieldName);
    
    protected string ReflectString(string fieldName) 
        => Source.GetReflectedValue<string>(fieldName) ?? string.Empty;
    
    protected string ReflectAsJson(string fieldName)
    {
        object? value = Source.GetReflectedValue(fieldName);

        return JsonConvert.SerializeObject(value);
    }
    
    protected string ReflectAsString(string fieldName)
    {
        object? value = Source.GetReflectedValue(fieldName);

        return GetAsString(value);
    }

    private static string GetAsString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string s)
        {
            return s;
        }

        if (value is ValueTuple<string, string>[] tuples)
        {
            return string.Join(',', tuples.Select(t => t.Item1 == t.Item2 ? t.Item1 : $"({t.Item1}, {t.Item2})"));
        }

        if (value is IEnumerable enumerable)
        {
            List<string> strs = new();
            foreach (var child in enumerable)
            {
                strs.Add(GetAsString(child));
            }

            return string.Join(',', strs);
        }

        if (value is object[] objArr)
        {
            return string.Join(", ", objArr);
        }

        if (value is IEnumerable<object> objEnum)
        {
            return string.Join(", ", objEnum);
        }

        return JsonConvert.SerializeObject(value);
    }
}