using System.Text;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public static class ValueToKeyMappingDetails
{
    /*
     * _unboundMaps: {"ItemType":{"RawType":"System.ReadOnlyMemory1[[System.Char, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"},"OutputType":{"Count":1030,"RawType":"System.UInt32, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"},"Count":1030}
       _textMetadata: false
       ColumnPairs: (Message_WordExtractor, Message_TextNormalizer_WordTokenizer)
     */
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string colPairs = fieldInfo["ColumnPairs"].Replace("(", "").Replace(")","");
        sb.Append($"Maps values in columns {colPairs} to keys in a bag of words");
    }
}