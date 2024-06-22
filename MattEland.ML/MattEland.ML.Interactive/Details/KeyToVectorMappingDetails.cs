using System.Text;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json;

namespace MattEland.ML.Interactive.Details;

public static class KeyToVectorMappingDetails
{
    public class KeyToVectorColumn
    {
        public string Name { get; set; }
        public string InputColumnName { get; set; }
        public bool OutputCountVector { get; set; }
    }
    /*[
       {
       "Name": "IsMerge",
       "InputColumnName": "IsMerge",
       "OutputCountVector": false
       },
       {
       "Name": "HasAddedFiles",
       "InputColumnName": "HasAddedFiles",
       "OutputCountVector": false
       },
       {
       "Name": "HasDeletedFiles",
       "InputColumnName": "HasDeletedFiles",
       "OutputCountVector": false
       }
       ]
     */
    public static void BuildNote(Dictionary<string, string> fieldInfo, StringBuilder sb)
    {
        string colPairs = fieldInfo["ColumnPairs"].Replace("(", "").Replace(")","");
        sb.Append($"Convert columns {colPairs} to an indicator vector");
    }
}