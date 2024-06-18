namespace MattEland.ML;

public class TrialMetricResult
{
    public int Id { get; set; }
    public double Metric { get; set; }
    public double BestMetric { get; set; }

    public override string ToString() => $"{Id} - {Metric:F3} (Best: {BestMetric:F3})";
}