namespace MattEland.ML.DataFrames;

public class DataFrameColumnTypes {
    public List<string> Text {get; set;} = new();
    public List<string> Numeric {get; set;} = new();
    public List<string> Categorical {get; set;} = new();
    public List<string> Excluded {get; set;} = new();
}