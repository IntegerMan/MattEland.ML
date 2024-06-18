using Microsoft.ML;
using Microsoft.ML.Runtime;

namespace MattEland.ML;

public class ContextMonitor : IDisposable
{
    private readonly MLContext _context;
    private StreamWriter? _file;
    private readonly object _lock = new();

    public ContextMonitor(MLContext context)
    {
        _context = context;
        _context.Log += OnLog;
        //_file = new StreamWriter(File.OpenWrite("MLContext.log"));
    }

    public List<TrialMetricResult> Results { get; } = new();
    public double? BestMetric { get; private set; }
    public bool MinimizeMetric { get; set; } = false;

    private void OnLog(object? sender, LoggingEventArgs e)
    {
        if (e.Source.Contains("AutoMLExperiment"))
        {
            if (e.Message == "[Source=AutoMLExperiment, Kind=Trace] Channel started")
            {
                Results.Clear();
                BestMetric = null;
            }
            
            if (e.Message.Contains("Update Completed Trial"))
            {
                (int trialId, double metric) = ParseCompletedExperiment(e.Message);
                
                if (BestMetric is null || (metric > BestMetric && !MinimizeMetric) || (metric < BestMetric && MinimizeMetric)) 
                {
                    BestMetric = metric;
                }
                
                Results.Add(new TrialMetricResult
                {
                    Id = trialId, 
                    Metric = metric, 
                    BestMetric = BestMetric.Value
                });
            }

            /*
            if (e.Kind >= ChannelMessageKind.Info)
            {
                lock (_lock)
                {
                    _file.WriteLine(e.Message);
                }
            }
            */
        }

    }

    private static (int, double) ParseCompletedExperiment(string message)
    {
        // Given a string like "[Source=AutoMLExperiment, Kind=Info] Update Completed Trial - Id: 54 - Metric: 0.6399999999999999 - Pipeline: Microsoft.ML.AutoML.SweepablePipeline - Duration: 96"
        string[] parts = message.Split(" - ");
        
        string idPart = parts[1];
        int id = int.Parse(idPart.Split(": ")[1]);
        
        string metricPart = parts[2];
        double metric = double.Parse(metricPart.Split(": ")[1]);
        
        return (id, metric);
    }

    public void Dispose()
    {
        _context.Log -= OnLog;
        _file?.Dispose();
    }
}