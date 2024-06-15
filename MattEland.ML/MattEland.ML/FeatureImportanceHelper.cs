using Microsoft.ML.AutoML;
using Microsoft.ML.Data;

namespace MattEland.ML;

public static class FeatureImportanceHelper
{
    public static IDictionary<string, double> ToImportancesDictionary(this IDictionary<string, BinaryClassificationMetricsStatistics> importance, BinaryClassificationMetric metric = BinaryClassificationMetric.F1Score, int features = 10, bool clean = true)
    {
        // Figure out the ones that impact the F1 score the most (positively or negatively)
        var orderedFeatures = importance.OrderByDescending(k =>
        {
            return metric switch
            {
                BinaryClassificationMetric.Accuracy => k.Value.Accuracy.Mean,
                BinaryClassificationMetric.AreaUnderRocCurve => k.Value.AreaUnderRocCurve.Mean,
                BinaryClassificationMetric.AreaUnderPrecisionRecallCurve => k.Value.AreaUnderPrecisionRecallCurve.Mean,
                BinaryClassificationMetric.F1Score => k.Value.F1Score.Mean,
                BinaryClassificationMetric.PositivePrecision => k.Value.PositivePrecision.Mean,
                BinaryClassificationMetric.PositiveRecall => k.Value.PositiveRecall.Mean,
                BinaryClassificationMetric.NegativePrecision => k.Value.NegativePrecision.Mean,
                BinaryClassificationMetric.NegativeRecall => k.Value.NegativeRecall.Mean,
                _ => throw new ArgumentOutOfRangeException(nameof(metric), metric, null)
            };
        });

        Dictionary<string, double> featureImpacts = new();
        foreach (var kvp in orderedFeatures.Take(features)) 
        {
            double avgImpact = kvp.Value.F1Score.Mean;

            string key = kvp.Key;
            if (clean)
            {
                key = key.Replace("|", "")
                    .Replace("<␂>", "*")
                    .Replace("<␠>", "*")
                    .Replace("<␃>", "*")
                    .Replace("Message.Word.", "")
                    .Replace("Message.Char.", "");
            }

            featureImpacts[key] = Math.Abs(avgImpact);
        }

        return featureImpacts;
    }
}