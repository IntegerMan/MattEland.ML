namespace MattEland.ML.Charts

open Microsoft.ML
open Microsoft.ML.Data
open Plotly.NET
open Plotly.NET.LayoutObjects

type MLCharts =
    static member private blues = StyleParam.Colorscale.Custom [ 0, Color.fromString("#FFFFFF"); 0.5, Color.fromString("#009dcf"); 1, Color.fromString("#003171") ]

    static member private RenderStandardConfusionMatrix tp fn fp tn =   
        let precisionP = float tp / (float tp + float fp)
        let precisionN = float tn / (float tn + float fn)
        let recallP = float tp / (float tp + float fn)
        let recallN = float tn / (float tn + float fp)

        let aTp = Annotation.init(X = 0, Y = 0, Text = $"%d{tp}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aFn = Annotation.init(X = 1, Y = 0, Text = $"%d{fn}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aFp = Annotation.init(X = 0, Y = 1, Text = $"%d{fp}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aTn = Annotation.init(X = 1, Y = 1, Text = $"%d{tn}", ShowArrow = false, BGColor = Color.fromString("#FFFFFF"), BorderColor = Color.fromString("#000099"), Font = Font.init(Size = 18))
        let aPrecP = Annotation.init(X = 0, Y = 1.55, Text = "Precision " + precisionP.ToString("F2"),  Font = Font.init(Size = 14), ShowArrow=false)
        let aPrecN = Annotation.init(X = 1, Y = 1.55, Text = "Precision " + precisionN.ToString("F2"),  Font = Font.init(Size = 14), ShowArrow=false)      
        let aRecP = Annotation.init(X = 1.55, Y = 0, Text = "Recall " + recallP.ToString("F2"),  Font = Font.init(Size = 14), ShowArrow=false, TextAngle = -90.0)
        let aRecN = Annotation.init(X = 1.55, Y = 1, Text = "Recall " + recallN.ToString("F2"),  Font = Font.init(Size = 14), ShowArrow=false, TextAngle = -90.0)      

        let matrix = [|
                        [| tp; fp |]
                        [| fn; tn |]
                     |]

        Chart.Heatmap(zData = matrix,
                      ColorScale = MLCharts.blues,
                      X = [ "Positive"; "Negative"],
                      Y = [ "Positive"; "Negative"],
                      ReverseYAxis = true)
        |> Chart.withTitle "Confusion Matrix" 
        |> Chart.withXAxisStyle(TitleText = "Predicted", Side = StyleParam.Side.Top, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withYAxisStyle(TitleText = "Actual", Side = StyleParam.Side.Left, ShowGrid = false, ShowBackground = false, ShowLine = false)
        |> Chart.withAnnotations([ aTp; aFn; aFp; aTn; aPrecP; aPrecN; aRecP; aRecN])
        
    static member private RenderNormalizedConfusionMatrix tp fn fp tn =
        let precisionP = float tp / (float tp + float fp)
        let precisionN = float tn / (float tn + float fn)
        let recallP = float tp / (float tp + float fn)
        let recallN = float tn / (float tn + float fp)

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
        let aPrecP = Annotation.init(X = 0, Y = 1.55, Text = "Precision " + precisionP.ToString("F2"),  Font = Font.init(Size = 14), ShowArrow=false)
        let aPrecN = Annotation.init(X = 1, Y = 1.55, Text = "Precision " + precisionN.ToString("F2"),  Font = Font.init(Size = 14), ShowArrow=false)      
        let aRecP = Annotation.init(X = 1.55, Y = 0, Text = "Recall " + recallP.ToString("F2"),  Font = Font.init(Size = 14), ShowArrow=false, TextAngle = -90.0)
        let aRecN = Annotation.init(X = 1.55, Y = 1, Text = "Recall " + recallN.ToString("F2"),  Font = Font.init(Size = 14), ShowArrow=false, TextAngle = -90.0)      

        Chart.Heatmap(zData = matrix, ColorScale = MLCharts.blues, X = classes, Y = classes, ReverseYAxis = true)
        |> Chart.withTitle "Normalized Confusion Matrix" 
        |> Chart.withXAxisStyle(TitleText = "Predicted", Side = StyleParam.Side.Top)
        |> Chart.withYAxisStyle(TitleText = "Actual", Side = StyleParam.Side.Left)
        |> Chart.withAnnotations([ aTp; aFn; aFp; aTn; aPrecP; aPrecN; aRecP; aRecN])
        
    static member RenderConfusionMatrix(cm : ConfusionMatrix, [<System.Runtime.InteropServices.Optional>] normalize: bool) =
        let tp = cm.GetCountForClassPair(0,0) |> int
        let tn = cm.GetCountForClassPair(1,1) |> int
        let fp = cm.GetCountForClassPair(0,1) |> int
        let fn = cm.GetCountForClassPair(1,0) |> int
        if normalize then
            MLCharts.RenderNormalizedConfusionMatrix tp fn fp tn
        else
            MLCharts.RenderStandardConfusionMatrix tp fn fp tn
