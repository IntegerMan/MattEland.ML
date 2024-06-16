using System.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public class TypeConvertDetails
{
    public string Name { get; set; }
    public string InputColumnName { get; set; }
    public Microsoft.ML.Data.DataKind OutputKind { get; set; }

    public override string ToString() 
        => Name == InputColumnName 
            ? $"{InputColumnName} to {OutputKind}" 
            : $"{InputColumnName} to {OutputKind} as {Name}";

    /* Sample:
     * {
       "Name": "IsMerge",
       "InputColumnName": "IsMerge",
       "OutputKind": 9,
       "OutputKeyCount": null
       },
       {
       "Name": "HasAddedFiles",
       "InputColumnName": "HasAddedFiles",
       "OutputKind": 9,
       "OutputKeyCount": null
       },
       {
       "Name": "HasDeletedFiles",
       "InputColumnName": "HasDeletedFiles",
       "OutputKind": 9,
       "OutputKeyCount": null
       }
     */
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string json = fieldInfo["_columns"];
        List<TypeConvertDetails> columns = JsonConvert.DeserializeObject<List<TypeConvertDetails>>(json) ?? [];
        sb.Append("Convert ");
        foreach (var col in columns)
        {
            sb.Append(col + (col == columns.Last() ? "" : ", "));
        }

        sb.AppendLine();
    }
}