namespace MattEland.ML.Charts

open Microsoft.ML
open Microsoft.ML.Data
open Plotly.NET
open Plotly.NET.LayoutObjects

module MLCharts =
    let blues = StyleParam.Colorscale.Custom [ 0, Color.fromString("#FFFFFF"); 0.5, Color.fromString("#009dcf"); 1, Color.fromString("#003171") ]

    let RenderStandardConfusionMatrix tp fn fp tn =   
        let matrix = [| [| tp; fp |]; [| fn; tn |] |]
        let classes = [ "Positive"; "Negative" ]

        let aTp = Annotation.init(X = 0, Y = 0, Text = $"%d{tp}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aFn = Annotation.init(X = 1, Y = 0, Text = $"%d{fn}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aFp = Annotation.init(X = 0, Y = 1, Text = $"%d{fp}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aTn = Annotation.init(X = 1, Y = 1, Text = $"%d{tn}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))

        Chart.Heatmap(zData = matrix, ColorScale = blues, X = classes, Y = classes, ReverseYAxis = true)
        |> Chart.withTitle "Confusion Matrix" 
        |> Chart.withXAxisStyle(TitleText = "Predicted", Side = StyleParam.Side.Top)
        |> Chart.withYAxisStyle(TitleText = "Actual", Side = StyleParam.Side.Left)
        |> Chart.withAnnotations([ aTp; aFn; aFp; aTn ])
        
    let RenderNormalizedConfusionMatrix tp fn fp tn =
        let total = float (tp + fn + fp + tn)
        let normalizedTp = (float tp / total)
        let normalizedFn = (float fn / total)
        let normalizedFp = (float fp / total)
        let normalizedTn = (float tn / total)

        let matrix = [| [| normalizedTp; normalizedFp |]; [| normalizedFn; normalizedTn |] |]
        let classes = [ "Positive"; "Negative" ]

        let aTp = Annotation.init(X = 0, Y = 0, Text = $"%.1f{normalizedTp}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aFn = Annotation.init(X = 1, Y = 0, Text = $"%.1f{normalizedFn}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aFp = Annotation.init(X = 0, Y = 1, Text = $"%.1f{normalizedFp}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aTn = Annotation.init(X = 1, Y = 1, Text = $"%.1f{normalizedTn}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))

        Chart.Heatmap(zData = matrix, ColorScale = blues, X = classes, Y = classes, ReverseYAxis = true)
        |> Chart.withTitle "Normalized Confusion Matrix" 
        |> Chart.withXAxisStyle(TitleText = "Predicted", Side = StyleParam.Side.Top)
        |> Chart.withYAxisStyle(TitleText = "Actual", Side = StyleParam.Side.Left)
        |> Chart.withAnnotations([ aTp; aFn; aFp; aTn ])
        
    // TODO: It'd be nice to have an overload that works with BinaryClassificationMetrics
    let RenderConfusionMatrix(cm : ConfusionMatrix, normalize: bool) =
        //let norm = defaultArg normalize false
        let tp = cm.GetCountForClassPair(0,0) |> int
        let tn = cm.GetCountForClassPair(1,1) |> int
        let fp = cm.GetCountForClassPair(0,1) |> int
        let fn = cm.GetCountForClassPair(1,0) |> int
        
        if normalize then
            RenderNormalizedConfusionMatrix tp fn fp tn
        else
            RenderStandardConfusionMatrix tp fn fp tn
