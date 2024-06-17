using System.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public static class ColumnSelectingDetails
{
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string cols = fieldInfo["_selectedColumns"];
        sb.AppendLine("Select columns " + cols);
    }
}