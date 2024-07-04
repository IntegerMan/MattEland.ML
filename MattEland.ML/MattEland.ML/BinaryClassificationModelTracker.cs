using System.Collections;
using Microsoft.Data.Analysis;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

namespace MattEland.ML;

public class BinaryClassificationModelTracker
{
    private readonly DataFrame _dataFrame;

    public BinaryClassificationModelTracker()
    {
        _dataFrame = new(
            new StringDataFrameColumn("Model"),
            new DoubleDataFrameColumn("F1 Score"),
            new DoubleDataFrameColumn("Accuracy"),
            new DoubleDataFrameColumn("Positive Precision"),
            new DoubleDataFrameColumn("Positive Recall"),
            new DoubleDataFrameColumn("Negative Precision"),
            new DoubleDataFrameColumn("Negative Recall"),
            new DoubleDataFrameColumn("AUC"),
            new DoubleDataFrameColumn("AUCPR")
        );
    }
    
    public BinaryClassificationModelTracker(DataFrame dataFrame)
    {
        _dataFrame = dataFrame;
    }
    
    public BinaryClassificationModelTracker Register(string modelName, BinaryClassificationMetrics metrics)
    {
        DataFrame appendDf = new DataFrame(
            new StringDataFrameColumn("Model", new[] { modelName }),
            new DoubleDataFrameColumn("F1 Score", new[] { metrics.F1Score }),
            new DoubleDataFrameColumn("Accuracy", new[] { metrics.Accuracy }),
            new DoubleDataFrameColumn("Positive Precision", new[] { metrics.PositivePrecision }),
            new DoubleDataFrameColumn("Positive Recall", new[] { metrics.PositiveRecall }),
            new DoubleDataFrameColumn("Negative Precision", new[] { metrics.NegativePrecision }),
            new DoubleDataFrameColumn("Negative Recall", new[] { metrics.NegativeRecall }),
            new DoubleDataFrameColumn("AUC", new[] { metrics.AreaUnderRocCurve }),
            new DoubleDataFrameColumn("AUCPR", new[] { metrics.AreaUnderPrecisionRecallCurve })
        );
        
        _dataFrame.Append(appendDf.Rows, inPlace: true);

        return this;
    }
    
    public BinaryClassificationModelTracker Merge(BinaryClassificationModelTracker other)
    {
        _dataFrame.Append(other._dataFrame.Rows, inPlace: true);
        return this;
    }
    
    public BinaryClassificationModelTracker Merge(DataFrame other)
    {
        _dataFrame.Append(other.Rows, inPlace: true);
        return this;
    }
    
    public BinaryClassificationModelTracker Register(string modelName, double truePositives, double falseNegatives, double falsePositives, double trueNegatives)
    {
        double f1Score = 2 * truePositives / (2 * truePositives + falsePositives + falseNegatives);
        double accuracy = (truePositives + trueNegatives) / (truePositives + falsePositives + falseNegatives + trueNegatives);
        double positivePrecision = truePositives / (truePositives + falsePositives);
        double positiveRecall = truePositives / (truePositives + falseNegatives);
        double negativePrecision = trueNegatives / (trueNegatives + falseNegatives);
        double negativeRecall = trueNegatives / (trueNegatives + falsePositives);
        double auc = (positiveRecall + negativeRecall) / 2;
        double aucpr = (positivePrecision + negativePrecision) / 2;
        
        DataFrame appendDf = new DataFrame(
            new StringDataFrameColumn("Model", new[] { modelName }),
            new DoubleDataFrameColumn("F1 Score", new[] { f1Score }),
            new DoubleDataFrameColumn("Accuracy", new[] { accuracy }),
            new DoubleDataFrameColumn("Positive Precision", new[] { positivePrecision }),
            new DoubleDataFrameColumn("Positive Recall", new[] { positiveRecall }),
            new DoubleDataFrameColumn("Negative Precision", new[] { negativePrecision }),
            new DoubleDataFrameColumn("Negative Recall", new[] { negativeRecall }),
            new DoubleDataFrameColumn("AUC", new[] { auc }),
            new DoubleDataFrameColumn("AUCPR", new[] { aucpr })
        );
        
        _dataFrame.Append(appendDf.Rows, inPlace: true);

        return this;
    }    
    
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
    
    //public (string, BinaryClassificationMetrics) GetBestBy() => GetBestBy(DefaultMetric);
    public BinaryClassificationMetric DefaultMetric { get; set; } = BinaryClassificationMetric.F1Score; 

    /*
    public (string, BinaryClassificationMetrics) GetBestBy(BinaryClassificationMetric metric)
    {
        if (!_dataFrame.Rows.Any())
            throw new InvalidOperationException("There must be at least one model tracked in order to use this method");
        
        KeyValuePair<string, BinaryClassificationMetrics> match = _trackedModels.MaxBy(kvp => GetMetricValue(kvp.Value, metric));

        return (match.Key, match.Value);
    }
    */

    public long Count => _dataFrame.Rows.Count;

    public DataFrame ToDataFrame() => _dataFrame;

    /*
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
    */
}