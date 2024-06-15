using Microsoft.Data.Analysis;

namespace MattEland.ML.DataFrames;

public static class DataFrameExtensions
{
    public static DataFrameColumnTypes GetColumnTypes(this DataFrame dataFrame, int maxValuesForCategorical = 15, params string[] excludedColumns)
    {
        DataFrameColumnTypes columnInfo = new();
        columnInfo.Excluded.AddRange(excludedColumns);

        foreach (var col in dataFrame.Columns)
        {
            if (columnInfo.Excluded.Contains(col.Name))
            {
                continue;
            }

            if (col.GetType() == typeof(StringDataFrameColumn))
            {
                if (col.ValueCounts().Rows.Count <= maxValuesForCategorical)
                {
                    columnInfo.Categorical.Add(col.Name);
                }
                else
                {
                    columnInfo.Text.Add(col.Name);
                }
            }
            else if (col.GetType() == typeof(BooleanDataFrameColumn) || 
                     col.GetType() == typeof(PrimitiveDataFrameColumn<bool>))
            {
                columnInfo.Categorical.Add(col.Name);
            }
            else
            {
                columnInfo.Numeric.Add(col.Name);
            }
        }

        return columnInfo;
    }
}