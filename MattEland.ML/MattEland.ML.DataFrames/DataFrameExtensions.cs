using Microsoft.Data.Analysis;

namespace MattEland.ML.DataFrames;

public static class DataFrameExtensions
{
    /// <summary>
    /// Removes one or more columns from a DataFrame.
    /// </summary>
    /// <param name="dataFrame">The DataFrame</param>
    /// <param name="columns">The columns to remove</param>
    /// <returns>A reference to <paramref name="dataFrame"/></returns>
    /// <remarks>
    /// Removing columns from a DataFrame is done in place. This does not create a new DataFrame.
    /// </remarks>
    public static DataFrame Remove(this DataFrame dataFrame, params string[] columns)
    {
        foreach (var column in columns)
        {
            dataFrame.Columns.Remove(column);
        }

        return dataFrame;
    }
}