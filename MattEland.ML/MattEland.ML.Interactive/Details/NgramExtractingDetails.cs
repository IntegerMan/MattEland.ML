using System.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public static class NgramExtractingDetails
{
    public class TransformInfos
    {
        public bool[] NonEmptyLevels { get; set; }
        public int NgramLength { get; set; }
        public int SkipLength { get; set; }
        public bool UseAllLengths { get; set; }
        public double Weighting { get; set; }
        public bool RequireIdf { get; set; }
    }

    public class NgramMaps
    {
        public int Count { get; set; }
    }
    
    /*
     * _transformInfos: {"NonEmptyLevels":[true,true],"NgramLength":2,"SkipLength":0,"UseAllLengths":true,"Weighting":0,"RequireIdf":false}
       _ngramMaps: {"Count":2480}
       ColumnPairs: Message_WordExtractor
     */
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string transformJson = fieldInfo["_transformInfos"];
        TransformInfos info = JsonConvert.DeserializeObject<TransformInfos>(transformJson) ?? new();

        string mapJson = fieldInfo["_ngramMaps"];
        NgramMaps maps = JsonConvert.DeserializeObject<NgramMaps>(mapJson) ?? new();

        sb.Append($"Extract up to {maps.Count} NGrams. ");

        if (info.UseAllLengths)
        {
            sb.Append($"NGrams can have lengths from {info.SkipLength} to {info.NgramLength}");
        }
        else
        {
            sb.Append($"NGrams must have a length of {info.NgramLength}");
        }
    }
}