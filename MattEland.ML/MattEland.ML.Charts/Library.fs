﻿namespace MattEland.ML.Charts

open System.Collections.Generic
open MattEland.ML
open Microsoft.ML
open Microsoft.ML.AutoML
open Microsoft.ML.Data
open Plotly.NET
open Plotly.NET.LayoutObjects
open Plotly.NET.StyleParam

type MLCharts =
    static member private blues = Colorscale.Custom [
        0,      Color.fromString("#FFFFFF")
        0.5,    Color.fromString("#009dcf")
        1,      Color.fromString("#003171")
    ]
    
    static member RenderBinaryConfusionMatrix(truePositives: float, falseNegatives: float, falsePositives: float, trueNegatives: float, includePrecisionRecall: bool, classNames: string seq) =
        let white = Color.fromString("#FFFFFF")
        let darkBlue = Color.fromString("#003171")
        let numFormat = "F0"
        
        let annotations =
            [
                yield Annotation.init(X = 0, Y = 0, Text = truePositives.ToString(numFormat), ShowArrow = false, BGColor = white, BorderColor = darkBlue, Font = Font.init(Size = 18))
                yield Annotation.init(X = 1, Y = 0, Text = falseNegatives.ToString(numFormat), ShowArrow = false, BGColor = white, BorderColor = darkBlue, Font = Font.init(Size = 18))
                yield Annotation.init(X = 1, Y = 1, Text = trueNegatives.ToString(numFormat), ShowArrow = false, BGColor = white, BorderColor = darkBlue, Font = Font.init(Size = 18))
                yield Annotation.init(X = 0, Y = 1, Text = falsePositives.ToString(numFormat), ShowArrow = false, BGColor = white, BorderColor = darkBlue, Font = Font.init(Size = 18))
            ]

        let matrix = [| [| truePositives; falseNegatives |]; [| falsePositives; trueNegatives |] |]

        let classNames = [| "Positive"; "Negative" |]
        
        Chart.Heatmap(zData = matrix,
                      ColorScale = MLCharts.blues,
                      // annotationText = text,
                      X = classNames,
                      Y = classNames,
                      ShowScale = false,
                      ReverseYAxis = true)
        |> Chart.withTitle "Confusion Matrix" 
        |> Chart.withXAxisStyle(TitleText = "Predicted", Side = Side.Top, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withYAxisStyle(TitleText = "Actual", Side = Side.Left, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withAnnotations(annotations)

    static member private RenderStandardConfusionMatrix(cm:ConfusionMatrix, includePrecisionRecall: bool, classNames: string seq) =
        let white = Color.fromString("#FFFFFF")
        let darkBlue = Color.fromString("#003171")
        let numFormat = "F0"
        let decFormat = "F3"
        
        let annotations =
            [
                for y = 0 to cm.Counts.Count - 1 do
                    for x = 0 to cm.Counts.Count - 1 do
                        let text = cm.GetCountForClassPair(x, y).ToString(numFormat)
                        yield Annotation.init(X = x, Y = y, Text = text, ShowArrow = false, BGColor = white, BorderColor = darkBlue, Font = Font.init(Size = 18))
                
                if includePrecisionRecall then
                    for c = 0 to cm.NumberOfClasses - 1 do                        
                        yield Annotation.init(X = c, Y = float cm.NumberOfClasses - 1. + 0.55, Text = "Precision " + cm.PerClassPrecision.[c].ToString(decFormat), ShowArrow = false, Font = Font.init(Size = 12))
                        yield Annotation.init(X = float cm.NumberOfClasses - 1. + 0.55, Y = c, Text = "Recall " + cm.PerClassRecall.[c].ToString(decFormat), ShowArrow = false, TextAngle = -90.0, Font = Font.init(Size = 12))
            ]

        let matrix = [| for y = 0 to cm.Counts.Count - 1 do
                         [| for x = 0 to cm.Counts.Count - 1 do
                              yield cm.GetCountForClassPair(x, y) |] |]

        Chart.Heatmap(zData = matrix,
                      ColorScale = MLCharts.blues,
                      //annotationText = text,
                      X = classNames,
                      Y = classNames,
                      ShowScale = false,
                      ReverseYAxis = true)
        |> Chart.withTitle "Confusion Matrix" 
        |> Chart.withXAxisStyle(TitleText = "Predicted", Side = Side.Top, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withYAxisStyle(TitleText = "Actual", Side = Side.Left, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withAnnotations(annotations)
        
    static member private RenderNormalizedConfusionMatrix(cm:ConfusionMatrix, includePrecisionRecall: bool, classNames: string seq) =
        let white = Color.fromString("#FFFFFF")
        let darkBlue = Color.fromString("#003171")
        let decFormat = "F3"

        let total = cm.Counts |> Seq.sumBy Seq.sum

        let annotations =
            [
                for y = 0 to cm.Counts.Count - 1 do
                    for x = 0 to cm.Counts.Count - 1 do
                        let value = cm.GetCountForClassPair(x, y) / float total
                        let text = value.ToString(decFormat)
                        yield Annotation.init(X = x, Y = y, Text = text, ShowArrow = false, BGColor = white, BorderColor = darkBlue, Font = Font.init(Size = 18))

                if includePrecisionRecall then
                    for c = 0 to cm.NumberOfClasses - 1 do                        
                        yield Annotation.init(X = c, Y = float cm.NumberOfClasses - 1. + 0.55, Text = "Precision " + cm.PerClassPrecision.[c].ToString(decFormat), ShowArrow = false, Font = Font.init(Size = 12))
                        yield Annotation.init(X = float cm.NumberOfClasses - 1. + 0.55, Y = c, Text = "Recall " + cm.PerClassRecall.[c].ToString(decFormat), ShowArrow = false, TextAngle = -90.0, Font = Font.init(Size = 12))
            ]

        let matrix = [| for y = 0 to cm.Counts.Count - 1 do
                         [| for x = 0 to cm.Counts.Count - 1 do
                              yield cm.GetCountForClassPair(x, y) / float total |] |]
        
        Chart.Heatmap(zData = matrix,
                      ColorScale = MLCharts.blues,
                      X = classNames,
                      Y = classNames,
                      ShowScale = false,
                      ReverseYAxis = true)
        |> Chart.withTitle "Normalized Confusion Matrix" 
        |> Chart.withXAxisStyle(TitleText = "Predicted", Side = StyleParam.Side.Top, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withYAxisStyle(TitleText = "Actual", Side = StyleParam.Side.Left, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withAnnotations(annotations)

        
    static member RenderConfusionMatrix(confusionMatrix : ConfusionMatrix, [<System.Runtime.InteropServices.Optional>] ?normalize: bool, [<System.Runtime.InteropServices.Optional>] ?classNames: string seq) =
        let norm = defaultArg normalize false
        let names = defaultArg classNames (seq { if confusionMatrix.NumberOfClasses = 2 then yield "Positive"; yield "Negative" else yield! [ for i in 0 .. confusionMatrix.NumberOfClasses - 1 -> i.ToString() ] })

        // TODO: Ideally, we could get the class names from the confusion matrix, but the PredictedClassesIndicators property is internal        
        if norm then
            MLCharts.RenderNormalizedConfusionMatrix(confusionMatrix, true, names)
        else
            MLCharts.RenderStandardConfusionMatrix(confusionMatrix, true, names)

    static member RenderConfusionMatrix(metrics : BinaryClassificationMetrics, [<System.Runtime.InteropServices.Optional>] ?normalize: bool) =
        let norm = defaultArg normalize false

        MLCharts.RenderConfusionMatrix(metrics.ConfusionMatrix, norm, [|"Positive"; "Negative"|])
        
    static member RenderConfusionMatrix(metrics : MulticlassClassificationMetrics, [<System.Runtime.InteropServices.Optional>] ?normalize: bool, [<System.Runtime.InteropServices.Optional>] ?classNames: string seq) =
        let norm = defaultArg normalize false
        let names = defaultArg classNames (seq { yield! [ for i in 0 .. metrics.ConfusionMatrix.NumberOfClasses - 1 -> i.ToString() ] })

        MLCharts.RenderConfusionMatrix(metrics.ConfusionMatrix, norm, names)        

    static member RenderConfusionMatrix(run : RunDetail<MulticlassClassificationMetrics>, [<System.Runtime.InteropServices.Optional>] ?normalize: bool, [<System.Runtime.InteropServices.Optional>] ?classNames: string seq) =
        let norm = defaultArg normalize false
        let names = defaultArg classNames (seq { yield! [ for i in 0 .. run.ValidationMetrics.ConfusionMatrix.NumberOfClasses - 1 -> i.ToString() ] })

        MLCharts.RenderConfusionMatrix(run.ValidationMetrics.ConfusionMatrix, norm, names)        

    static member RenderConfusionMatrix(run : RunDetail<BinaryClassificationMetrics>, [<System.Runtime.InteropServices.Optional>] ?normalize: bool) =
        let norm = defaultArg normalize false

        MLCharts.RenderConfusionMatrix(run.ValidationMetrics.ConfusionMatrix, norm, [| "Positive"; "Negative"|])    

    static member RenderClassificationMetrics(metrics: BinaryClassificationMetrics) =
        let keys = [
            "Negative Recall";
            "Negative Precision";
            "Positive Recall";
            "Positive Precision";
            "AUCPR";    // Area under the Precision-Recall curve
            "AUC";      // Area under the ROC curve
            "F1 Score";
            "Accuracy"
        ]
        let values = [
            metrics.NegativeRecall
            metrics.NegativePrecision;
            metrics.PositiveRecall;
            metrics.PositivePrecision;
            metrics.AreaUnderPrecisionRecallCurve;
            metrics.AreaUnderRocCurve;
            metrics.F1Score;
            metrics.Accuracy;
        ]
        let text = values |> Seq.map(_.ToString("F3"))
        Chart.Bar(values = values, Keys = keys, MultiText = text, MarkerColor = Color.fromString("#003171"))
        |> Chart.withTitle "Binary Classification Metrics"
        |> Chart.withXAxisStyle(MinMax=(0, 1))
        
    static member RenderPartialClassificationMetrics(metrics: BinaryClassificationMetrics) =
        let keys = [
            "AUCPR";    // Area under the Precision-Recall curve
            "AUC";      // Area under the ROC curve
            "F1 Score";
            "Accuracy"
        ]
        let values = [
            metrics.AreaUnderPrecisionRecallCurve;
            metrics.AreaUnderRocCurve;
            metrics.F1Score;
            metrics.Accuracy;
        ]
        let text = values |> Seq.map(_.ToString("F3"))
        Chart.Bar(values = values, Keys = keys, MultiText = text, MarkerColor = Color.fromString("#003171"))
        |> Chart.withTitle "Binary Classification Metrics"
        |> Chart.withXAxisStyle(MinMax=(0, 1))
        
    static member RenderClassificationMetrics(run: RunDetail<BinaryClassificationMetrics>) =
        MLCharts.RenderClassificationMetrics(run.ValidationMetrics)
        
    static member RenderClassificationMetrics(metrics: MulticlassClassificationMetrics) =
        let keys = [
            "Macro Accuracy";
            "Micro Accuracy";
            "Log Loss";
            "Top K Accuracy"
        ]
        let values = [
            metrics.MacroAccuracy;
            metrics.MicroAccuracy
            metrics.LogLoss
            metrics.TopKAccuracy
        ]
        Chart.Bar(values = values, Keys = keys)
        |> Chart.withTitle "Multiclass Classification Metrics"
        |> Chart.withXAxisStyle(MinMax=(0, 1))
        
    static member RenderClassificationMetrics(run: RunDetail<MulticlassClassificationMetrics>) =
        MLCharts.RenderClassificationMetrics(run.ValidationMetrics)

    static member ClassificationReport(metrics : BinaryClassificationMetrics) =
        let confusionMatrix: GenericChart = MLCharts.RenderStandardConfusionMatrix(metrics.ConfusionMatrix, false, [|"Positive"; "Negative"|])
        let metricsChart = MLCharts.RenderClassificationMetrics(metrics)
        
        [ confusionMatrix; metricsChart ]
        |> Chart.Grid(nRows = 2, nCols=1,
                      SubPlotTitles = [""; "Binary Classification Metric"],
                      Pattern = LayoutGridPattern.Independent)
        |> Chart.withTitle "Classification Report"
        |> Chart.withMarkerStyle (ShowScale = false)        
        
    static member ClassificationReport(run : RunDetail<BinaryClassificationMetrics>) =
        MLCharts.ClassificationReport(run.ValidationMetrics)        
        
    static member NormalizedClassificationReport(run : RunDetail<BinaryClassificationMetrics>) =
        let confusionMatrix: GenericChart = MLCharts.RenderNormalizedConfusionMatrix(run.ValidationMetrics.ConfusionMatrix, false, [|"Positive"; "Negative"|])
        let metricsChart = MLCharts.RenderClassificationMetrics(run.ValidationMetrics)
        
        [ confusionMatrix; metricsChart ]
        |> Chart.Grid(nRows = 2, nCols=1,
                      SubPlotTitles = [""; "Binary Classification Metric"],
                      Pattern = LayoutGridPattern.Independent)
        |> Chart.withTitle "Classification Report"
        |> Chart.withMarkerStyle (ShowScale = false)

    static member FeatureImportances(importances: IDictionary<string, float>, metric: string) =
        Chart2D.Chart.Column(
            values=importances.Values,
            Keys=importances.Keys)
            |> Chart.withTitle("Feature Importances")
            |> Chart.withXAxisStyle (TitleText = "Feature")
            |> Chart.withYAxisStyle (TitleText = "Absolute impact on " + metric)
            |> Chart.withSize(Width = 800)

    static member FeatureImportances(context: MLContext, model: ITransformer, data: IDataView, permutationCount: int, numFeatures: int) =
        let transformed: IDataView = model.Transform data
        let imps = context.BinaryClassification.PermutationFeatureImportanceNonCalibrated(model, transformed, permutationCount = permutationCount)
        let dict = FeatureImportanceHelper.ToImportancesDictionary(imps, features = numFeatures)
        MLCharts.FeatureImportances(dict, "F1 Score")
     
    static member MetricImprovement(monitor: ContextMonitor) =
         let results = List.ofSeq monitor.Results
         let ids = results |> List.map (_.Id)
         let bestVals = results |> List.map(_.BestMetric)
         
         Chart.Line(x = ids, y = bestVals)
         |> Chart.withTitle("Best Model Improvement over Multiple Trials")
         |> Chart.withXAxisStyle (TitleText = "Trial Number")
         |> Chart.withYAxisStyle (TitleText = "Primary Metric Value")
         |> Chart.withSize(Width = 800)
         
    static member MetricImprovementWithTrials(monitor: ContextMonitor) =
         let results = List.ofSeq monitor.Results
         let ids = results |> List.map (_.Id)
         let bestVals = results |> List.map(_.BestMetric)
         let metrics = results |> List.map(_.Metric)
         
         [
            Chart.Line(x = ids, y = bestVals, Name="Best Result")
            |> Chart.withLineStyle (Width = 2., Dash = DrawingStyle.Dot)
            
            Chart.Point(x = ids, y = metrics, Name="Trial Result")
         ]
         |> Chart.combine
         |> Chart.withTitle("Trial Metrics over Multiple Trials")
         |> Chart.withXAxisStyle (TitleText = "Trial Number")
         |> Chart.withYAxisStyle (TitleText = "Primary Metric Value")
         |> Chart.withSize(Width = 800)
