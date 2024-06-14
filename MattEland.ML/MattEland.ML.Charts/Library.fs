namespace MattEland.ML.Charts

open Microsoft.ML
open Microsoft.ML.AutoML
open Microsoft.ML.Data
open Plotly.NET
open Plotly.NET.LayoutObjects

type MLCharts =
    static member private blues = StyleParam.Colorscale.Custom [ 0, Color.fromString("#FFFFFF"); 0.5, Color.fromString("#009dcf"); 1, Color.fromString("#003171") ]

    static member private RenderStandardConfusionMatrix(cm:ConfusionMatrix, classNames: string seq) =
        let white = Color.fromString("#FFFFFF")
        let darkBlue = Color.fromString("#000099")
        let numFormat = "F0"
        let decFormat = "F2"
        
        let annotations =
            [
                for y = 0 to cm.Counts.Count - 1 do
                    for x = 0 to cm.Counts.Count - 1 do
                        let text = cm.GetCountForClassPair(x, y).ToString(numFormat)
                        yield Annotation.init(X = x, Y = y, Text = text, ShowArrow = false, BGColor = white, BorderColor = darkBlue, Font = Font.init(Size = 18))

                for c = 0 to cm.NumberOfClasses - 1 do                        
                    yield Annotation.init(X = c, Y = float cm.NumberOfClasses - 1. + 0.55, Text = "Precision " + cm.PerClassPrecision.[c].ToString(decFormat), ShowArrow = false, Font = Font.init(Size = 14))
                    yield Annotation.init(X = float cm.NumberOfClasses - 1. + 0.55, Y = c, Text = "Recall " + cm.PerClassRecall.[c].ToString(decFormat), ShowArrow = false, TextAngle = -90.0, Font = Font.init(Size = 14))
            ]

        let matrix = [| for y = 0 to cm.Counts.Count - 1 do
                         [| for x = 0 to cm.Counts.Count - 1 do
                              yield cm.GetCountForClassPair(x, y) |] |]
        
        Chart.Heatmap(zData = matrix,
                      ColorScale = MLCharts.blues,
                      X = classNames,
                      Y = classNames,
                      ReverseYAxis = true)
        |> Chart.withTitle "Confusion Matrix" 
        |> Chart.withXAxisStyle(TitleText = "Predicted", Side = StyleParam.Side.Top, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withYAxisStyle(TitleText = "Actual", Side = StyleParam.Side.Left, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withAnnotations(annotations)
        
    static member private RenderNormalizedConfusionMatrix(cm:ConfusionMatrix, classNames: string seq) =
        let white = Color.fromString("#FFFFFF")
        let darkBlue = Color.fromString("#000099")
        let decFormat = "F2"

        let total = cm.Counts |> Seq.sumBy (fun counts -> Seq.sum counts)

        let annotations =
            [
                for y = 0 to cm.Counts.Count - 1 do
                    for x = 0 to cm.Counts.Count - 1 do
                        let value = cm.GetCountForClassPair(x, y) / float total
                        let text = value.ToString(decFormat)
                        yield Annotation.init(X = x, Y = y, Text = text, ShowArrow = false, BGColor = white, BorderColor = darkBlue, Font = Font.init(Size = 18))

                for c = 0 to cm.NumberOfClasses - 1 do                        
                    yield Annotation.init(X = c, Y = float cm.NumberOfClasses - 1. + 0.55, Text = "Precision " + cm.PerClassPrecision.[c].ToString(decFormat), ShowArrow = false, Font = Font.init(Size = 14))
                    yield Annotation.init(X = float cm.NumberOfClasses - 1. + 0.55, Y = c, Text = "Recall " + cm.PerClassRecall.[c].ToString(decFormat), ShowArrow = false, TextAngle = -90.0, Font = Font.init(Size = 14))
            ]

        let matrix = [| for y = 0 to cm.Counts.Count - 1 do
                         [| for x = 0 to cm.Counts.Count - 1 do
                              yield cm.GetCountForClassPair(x, y) / float total |] |]
        
        Chart.Heatmap(zData = matrix,
                      ColorScale = MLCharts.blues,
                      X = classNames,
                      Y = classNames,
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
            MLCharts.RenderNormalizedConfusionMatrix(confusionMatrix, names)
        else
            MLCharts.RenderStandardConfusionMatrix(confusionMatrix, names)

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
