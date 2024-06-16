using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public class ColumnConcatDetails
{
    public string Name { get; set; }
    public List<ValueTuple<string, string>> Sources { get; set; }

    public override string ToString() 
        => $"Concat {string.Join(", ", Sources.Select(s => s.Item1))} to {Name}";
    
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string json = fieldInfo["_columns"];
        List<ColumnConcatDetails> details = JsonConvert.DeserializeObject<List<ColumnConcatDetails>>(json) ?? new();
        foreach (var det in details)
        {
            sb.AppendLine(det.ToString());
        }
    }
}