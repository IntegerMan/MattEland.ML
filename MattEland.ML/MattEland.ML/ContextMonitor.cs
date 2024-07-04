using System.Net.Mime;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.ML.SearchSpace.Option;
using Microsoft.ML.Trainers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MattEland.ML;

public class ContextMonitor : IDisposable
{
    private readonly MLContext _context;
    private readonly Dictionary<int, ExperimentTrial> _trials = new();

    public ContextMonitor(MLContext context)
    {
        _context = context;
        _context.Log += OnLog;
    }

    public IEnumerable<ExperimentTrial> Results => _trials.Values;
    public double? BestMetric => BestTrial?.Metric;
    public ExperimentTrial? BestTrial { get; private set; }
    public bool MinimizeMetric { get; set; } = false;

    private void OnLog(object? sender, LoggingEventArgs e)
    {
        if (e.Source.Contains("AutoMLExperiment"))
        {
            if (e.Message == "[Source=AutoMLExperiment, Kind=Trace] Channel started")
            {
                _trials.Clear();
                BestTrial = null;
            }

            if (e.Message.Contains("trial setting", StringComparison.OrdinalIgnoreCase))
            {
                ExperimentTrial trial = ParseTrial(e.Message);
                _trials[trial.Id] = trial;
            } 
            else if (e.Message.Contains("Update Completed Trial", StringComparison.OrdinalIgnoreCase))
            {
                (int trialId, double metric) = ParseCompletedExperiment(e.Message);

                ExperimentTrial trial = _trials[trialId];
                
                if (BestMetric is null || (metric > BestMetric && !MinimizeMetric) || (metric < BestMetric && MinimizeMetric)) 
                {
                    BestTrial = trial;
                }

                trial.Metric = metric;
                trial.BestMetric = BestMetric!.Value;
            }
        }
    }

    private static (int, double) ParseCompletedExperiment(string message)
    {
        // Parses strings like "[Source=AutoMLExperiment, Kind=Info] Update Completed Trial - Id: 54 - Metric: 0.6399999999999999 - Pipeline: Microsoft.ML.AutoML.SweepablePipeline - Duration: 96"
        string[] parts = message.Split(" - ");
        
        string idPart = parts[1];
        int id = int.Parse(idPart.Split(": ")[1]);
        
        string metricPart = parts[2];
        double metric = double.Parse(metricPart.Split(": ")[1]);
        
        return (id, metric);
    }
    
    private static ExperimentTrial ParseTrial(string message)
    {
        // Parses strings like "[Source=AutoMLExperiment, Kind=Trace] trial setting - {"TrialId":2,"StartedAtUtc":"2024-06-26T04:25:50.1218934Z","EndedAtUtc":null,"Parameter":{"_pipeline_":{"_SCHEMA_":"e0 * e1","e0":{},"e1":{"FeatureFraction":0.79584885,"NumberOfLeaves":34,"NumberOfTrees":2}},"_SCHEMA_":"e0 * e1","e0":{},"e1":{"FeatureFraction":1,"NumberOfLeaves":30,"NumberOfTrees":4}}}"
        var start = message.IndexOf('{');
        var end = message.LastIndexOf('}');
        var json = message.Substring(start, end-start+1);
        var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)!;
        int trialId = int.Parse(obj["TrialId"].ToString()!);
        JObject param = (JObject)obj["Parameter"];
        param = (JObject)param["_pipeline_"]!;

        // Iterate over each key in param aside from _SCHEMA_ and add its value to a dictionary
        HyperparameterCollection values = new();
        foreach (var key in param.Properties())
        {
            if (key.Name != "_SCHEMA_")
            {
                JObject prop = (JObject)param[key.Name]!;
                foreach (var propKey in prop.Properties())
                {
                    values[propKey.Name] = prop[propKey.Name];
                }
            }
        }
        
        return new ExperimentTrial(trialId, values);    
    }

    public void Dispose()
    {
        _context.Log -= OnLog;
    }
}
