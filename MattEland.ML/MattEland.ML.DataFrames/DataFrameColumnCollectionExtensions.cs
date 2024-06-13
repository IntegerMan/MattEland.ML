using Microsoft.Data.Analysis;

namespace MattEland.ML.DataFrames;

public static class DataFrameColumnCollectionExtensions
{
    /// <summary>
    /// Removes one or more columns from a DataFrame and returns the count removed.
    /// </summary>
    /// <param name="columns">The collection of columns</param>
    /// <param name="columnsToRemove">The columns to remove</param>
    /// <returns>The number of columns actually removed.</returns>
    public static int Remove(this DataFrameColumnCollection columns, params string[] columnsToRemove)
    {
        int removed = 0;
        foreach (var column in columnsToRemove)
        {
            var match = columns.FirstOrDefault(c => c.Name == column);
            if (match != null)
            {
                columns.Remove(match);
                removed++;
            }
        }

        return removed;
    }
    
    /// <summary>
    /// Removes all columns a DataFrame except those explicitly named as ones to keep and returns the count removed.
    /// </summary>
    /// <param name="columns">The collection of columns</param>
    /// <param name="columnsToKeep">The columns to keep</param>
    /// <returns>The number of columns actually removed.</returns>
    public static int RemoveAllBut(this DataFrameColumnCollection columns, params string[] columnsToKeep)
    {
        int removed = 0;
        DataFrameColumn[] existingColumns = columns.ToArray();
        foreach (var column in existingColumns.Where(c => !columnsToKeep.Contains(c.Name)))
        {
            columns.Remove(column);
            removed++;
        }

        return removed;
    }    
}