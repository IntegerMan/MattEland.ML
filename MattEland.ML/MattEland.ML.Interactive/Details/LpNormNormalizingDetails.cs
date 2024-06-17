using System.Text;
using Microsoft.ML.Transforms;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public static class LpNormNormalizingDetails
{
    public class NormalizingColumn
    {
        public string Name { get; set; }
        public string InputColumnName { get; set; }
        public LpNormNormalizingEstimatorBase.NormFunction Norm { get; set; }
        public bool EnsureZeroMean { get; set; }
        public double Scale { get; set; }
    }
    /*
     * {
       "Name": "Message_CharExtractor_LpCharNorm",
       "InputColumnName": "Message_CharExtractor",
       "Norm": 0,
       "EnsureZeroMean": false,
       "Scale": 1.0
       },
       {
       "Name": "Message_WordExtractor_LpWordNorm",
       "InputColumnName": "Message_WordExtractor",
       "Norm": 0,
       "EnsureZeroMean": false,
       "Scale": 1.0
       }
     */
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string columnsJson = fieldInfo["_columns"];
        List<NormalizingColumn> columns = JsonConvert.DeserializeObject<List<NormalizingColumn>>(columnsJson) ?? new();

        foreach (var col in columns)
        {
            sb.Append($"Normalize {col.InputColumnName} via {col.Norm} normalization");
            if (col.InputColumnName != col.Name)
            {
                sb.Append($" as {col.Name}");
            }

            if (col.EnsureZeroMean)
            {
                sb.Append(" ensuring a zero mean");
            }

            if (Math.Abs(col.Scale - 1.0) > double.Epsilon)
            {
                sb.Append(" scaling by " + col.Scale + "x");
            }

            sb.AppendLine();
        }
    }
}