using Microsoft.ML;

namespace MattEland.ML;

public static class ContextExtensions
{
    public static ContextMonitor Monitor(this MLContext context)
    {
        return new ContextMonitor(context);
    }
}