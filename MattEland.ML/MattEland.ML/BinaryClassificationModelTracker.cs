using System.Collections;
using Microsoft.Data.Analysis;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

namespace MattEland.ML;

public class BinaryClassificationModelTracker : IEnumerable<KeyValuePair<string, BinaryClassificationMetrics>>
{
    private readonly Dictionary<string, BinaryClassificationMetrics> _trackedModels = new();

    public BinaryClassificationModelTracker Register(string modelName, BinaryClassificationMetrics metrics)
    {
        _trackedModels[modelName] = metrics;

        return this;
    }    
    
    public BinaryClassificationModelTracker Register(string modelName, CrossValidationRunDetail<BinaryClassificationMetrics> run)
    {
        _trackedModels[modelName] = run.Results.First().ValidationMetrics;

        return this;
    }

    public BinaryClassificationModelTracker Register(string modelName, TrainResult<BinaryClassificationMetrics> run)
    {
        _trackedModels[modelName] = run.ValidationMetrics;
        
        return this;
    }
    
    public BinaryClassificationModelTracker Register(string modelName, RunDetail<BinaryClassificationMetrics> run)
    {
        _trackedModels[modelName] = run.ValidationMetrics;
        
        return this;
    }
    
    public IEnumerator<KeyValuePair<string, BinaryClassificationMetrics>> GetEnumerator() => _trackedModels.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_trackedModels).GetEnumerator();

    private static double GetMetricValue(BinaryClassificationMetrics metrics,
        BinaryClassificationMetric metricOfInterest)
    {
        return metricOfInterest switch
        {
            BinaryClassificationMetric.Accuracy => metrics.Accuracy,
            BinaryClassificationMetric.AreaUnderRocCurve => metrics.AreaUnderRocCurve,
            BinaryClassificationMetric.AreaUnderPrecisionRecallCurve => metrics.AreaUnderPrecisionRecallCurve,
            BinaryClassificationMetric.F1Score => metrics.F1Score,
            BinaryClassificationMetric.PositivePrecision => metrics.PositivePrecision,
            BinaryClassificationMetric.PositiveRecall => metrics.PositiveRecall,
            BinaryClassificationMetric.NegativePrecision => metrics.NegativePrecision,
            BinaryClassificationMetric.NegativeRecall => metrics.NegativeRecall,
            _ => throw new ArgumentOutOfRangeException(nameof(metricOfInterest), metricOfInterest, null)
        };
    }
    
    public (string, BinaryClassificationMetrics) GetBestBy() => GetBestBy(DefaultMetric);
    public BinaryClassificationMetric DefaultMetric { get; set; } = BinaryClassificationMetric.F1Score; 

    public (string, BinaryClassificationMetrics) GetBestBy(BinaryClassificationMetric metric)
    {
        if (!_trackedModels.Any())
            throw new InvalidOperationException("There must be at least one model tracked in order to use this method");
        
        KeyValuePair<string, BinaryClassificationMetrics> match = _trackedModels.MaxBy(kvp => GetMetricValue(kvp.Value, metric));

        return (match.Key, match.Value);
    }

    public int Count => _trackedModels.Count;

    public DataFrame ToDataFrame() 
        => new DataFrame(
            new StringDataFrameColumn("Model", _trackedModels.Keys),
            new DoubleDataFrameColumn("F1 Score", _trackedModels.Values.Select(v => v.F1Score)),
            new DoubleDataFrameColumn("Accuracy", _trackedModels.Values.Select(v => v.Accuracy)),
            new DoubleDataFrameColumn("Positive Precision", _trackedModels.Values.Select(v => v.PositivePrecision)),
            new DoubleDataFrameColumn("Positive Recall", _trackedModels.Values.Select(v => v.PositiveRecall)),
            new DoubleDataFrameColumn("Negative Precision", _trackedModels.Values.Select(v => v.NegativePrecision)),
            new DoubleDataFrameColumn("Negative Recall", _trackedModels.Values.Select(v => v.NegativeRecall)),
            new DoubleDataFrameColumn("AUC", _trackedModels.Values.Select(v => v.AreaUnderRocCurve)),
            new DoubleDataFrameColumn("AUCPR", _trackedModels.Values.Select(v => v.AreaUnderPrecisionRecallCurve))
        );

    public void Clear()
    {
        _trackedModels.Clear();
    }

    public override string ToString()
    {
        switch (Count)
        {
            case 0:
                return "No Models Tracked";
            case 1:
                return $"{_trackedModels.Keys.First()}: {GetMetricValue(_trackedModels.Values.First(), DefaultMetric)} {DefaultMetric}";
            default:
                (string name, BinaryClassificationMetrics metrics) = GetBestBy(DefaultMetric);
                return $"{Count} Models. Best: {name} with {GetMetricValue(metrics, DefaultMetric)} {DefaultMetric}";
        }
    }
}