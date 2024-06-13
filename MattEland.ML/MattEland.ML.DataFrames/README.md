# MattEland.ML.DataFrames

This library offers extensions to make working with the `Microsoft.Data.Analysis.DataFrame` class easier for data wrangling and machine learning workloads.

This code is in early prototyping and the library is not yet meant to be broadly consumed.

## Capabilities

The library currently offers the following capabilities:

### Removing multiple columns from a DataFrame

Previously in order to remove multiple columns from a DataFrame you needed to create an enumerable and loop over it, removing each column in turn.

This library provides a `myDataFrame.Columns.Remove` extension method that takes in a params array of column names to remove.

Usage:
```cs
int columnsRemoved = myDataFrame.Columns.Remove("ColumnA", "ColumnB", "ColumnC");
```

Additionally, you can remove all columns except for a few specified columns with the `myDataFrame.Columns.RemoveAllBut` method.

This is particularly helpful when you want drop all columns not needed for model training.

Usage:
```cs
int columnsRemoved = myDataFrame.Columns.RemoveAllBut("ColumnC", "ColumnD");
```