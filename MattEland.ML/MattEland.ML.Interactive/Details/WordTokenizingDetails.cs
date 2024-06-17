using System.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public static class WordTokenizingDetails
{
    public class WordTokenizingColumn
    {
        public string Name { get; set; }
        public string InputColumnName { get; set; }
        public List<string> Separators { get; set; }
    }
    
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string columnsJson = fieldInfo["_columns"];
        List<WordTokenizingColumn> columns = JsonConvert.DeserializeObject<List<WordTokenizingColumn>>(columnsJson) ?? new();

        foreach (var col in columns)
        {
            sb.Append($"Tokenize {col.InputColumnName}");
            if (col.InputColumnName != col.Name)
            {
                sb.Append($" as {col.Name}");
            }

            if (col.Separators is not [" "])
            {
                sb.Append(" using separators " + string.Join(", ", col.Separators));
            }
        }
    }
}