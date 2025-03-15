using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Overlays;

namespace MemeFactory.Core.Utilities;

public static class Brushes
{
    private static readonly GraphicsOptions DefaultGraphicsOptions = new GraphicsOptions
    {
        Antialias = true,
    };

    public static IAsyncEnumerable<Frame> BackgroundColor(this IAsyncEnumerable<Frame> frames, Color color,
        GraphicsOptions? graphicsOptions = null)

    {
        var processor = new BackgroundColorProcessor(graphicsOptions ?? DefaultGraphicsOptions, color);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> Glow(this IAsyncEnumerable<Frame> frames, Color color, GraphicsOptions? graphicsOptions = null)
    {
        var processor = new GlowProcessor(graphicsOptions ?? DefaultGraphicsOptions, color);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }


    public static IAsyncEnumerable<Frame> Vignette(this IAsyncEnumerable<Frame> frames, Color color, GraphicsOptions? graphicsOptions = null)
    {
        var processor = new VignetteProcessor(graphicsOptions ?? DefaultGraphicsOptions, color);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
}