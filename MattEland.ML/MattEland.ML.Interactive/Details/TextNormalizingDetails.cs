using System.Text;
using Microsoft.ML.Transforms.Text;

namespace MattEland.ML.Interactive.Details;

public static class TextNormalizingDetails
{
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        TextNormalizingEstimator.CaseMode caseMode = Enum.Parse<TextNormalizingEstimator.CaseMode>(fieldInfo["_caseMode"]);
        bool keepDiacritics = bool.Parse(fieldInfo["_keepDiacritics"]);
        bool keepPunctuation = bool.Parse(fieldInfo["_keepPunctuations"]);
        bool keepNumbers = bool.Parse(fieldInfo["_keepNumbers"]);
        string columns = fieldInfo["ColumnPairs"].Replace("(", "").Replace(")","");

        sb.Append("On columns " + columns + ": ");
        List<string> ops = new();
        if (!keepPunctuation)
        {
            ops.Add("remove punctuation");
        }

        if (!keepNumbers)
        {
            ops.Add("remove numbers");
        }

        if (!keepDiacritics)
        {
            ops.Add("remove diacritics");
        }

        if (caseMode == TextNormalizingEstimator.CaseMode.Lower)
        {
            ops.Add("convert to lowercase");
        }
        else if (caseMode == TextNormalizingEstimator.CaseMode.Upper)
        {
            ops.Add("convert to uppercase");
        }

        sb.Append(string.Join(", ", ops));
    }
}