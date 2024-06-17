using System.Text;

namespace MattEland.ML.Interactive.Details;

public static class MissingValuesDetails
{
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string[] values = fieldInfo["_repValues"].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var groupedValue in values.GroupBy(v => v))
        {
            sb.AppendLine($"Replace missing values in {groupedValue.Count()} column(s) with {groupedValue.Key}");
        }
    }
}