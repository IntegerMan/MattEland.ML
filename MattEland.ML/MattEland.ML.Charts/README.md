# MattEland.ML.Charts

This library offers data science charts for ML.NET and AutoML.

Charts are rendered using Plotly.NET

This code is in early prototyping and the library is not yet meant to be broadly consumed.

## Capabilities

The library currently offers the following capabilities:

### Confusion Matrix

The Confusion Matrix renders a graphical shaded confusion matrix which includes per-class precision and recall.

Usage:
```cs
MLCharts.RenderConfusionMatrix(results.BestRun.ValidationMetrics.ConfusionMatrix)
```

There's also an optional parameter to use the normalized variant:

```cs
MLCharts.RenderConfusionMatrix(results.BestRun.ValidationMetrics.ConfusionMatrix, normalized: true)
```

There are overloads available for `BinaryValidationMetrics`, `MulticlassClassificationMetrics`, `RunDetails<BinaryClassificationMetrics>`, and `RunDetails<MulticlassClassificationMetrics>`
