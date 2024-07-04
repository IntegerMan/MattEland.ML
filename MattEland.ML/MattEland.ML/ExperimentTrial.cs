namespace MattEland.ML;

public class ExperimentTrial
{
    public int Id { get; }
    public HyperparameterCollection Hyperparameters { get; }
    public double BestMetric { get; set; }
    public double Metric { get; set; }

    public ExperimentTrial(int id, HyperparameterCollection hyperparameters)
    {
        Id = id;
        Hyperparameters = hyperparameters;
    }
}

public class HyperparameterCollection : Dictionary<string, object?>
{
    
}