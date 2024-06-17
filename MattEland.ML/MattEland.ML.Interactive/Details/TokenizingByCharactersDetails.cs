using System.Text;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public static class TokenizingByCharactersDetails
{
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        bool useMarkerChars = bool.Parse(fieldInfo["_useMarkerChars"]);
        string colPairs = fieldInfo["ColumnPairs"].Replace("(", "").Replace(")","");

        sb.Append("Tokenize columns " + colPairs);
        if (useMarkerChars)
        {
            sb.Append(" using marker characters");
        }
    }
}