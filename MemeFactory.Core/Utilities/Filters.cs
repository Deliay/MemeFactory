using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using SixLabors.ImageSharp.Processing.Processors.Effects;
using SixLabors.ImageSharp.Processing.Processors.Filters;
using SixLabors.ImageSharp.Processing.Processors.Overlays;

namespace MemeFactory.Core.Utilities;

public static class Filters
{
    public static IAsyncEnumerable<Frame> BokehBlur(this IAsyncEnumerable<Frame> frames, int radius = 32, int components = 2, float gamma = 3f)
    {
        var processor = new BokehBlurProcessor(radius, components, gamma);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> GaussianBlur(this IAsyncEnumerable<Frame> frames, float sigma = 3f, int radius = 9)
    {
        var processor = new GaussianBlurProcessor(sigma, radius);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> GaussianSharpen(this IAsyncEnumerable<Frame> frames, float sigma = 3f, int radius = 9)
    {
        var processor = new GaussianSharpenProcessor(sigma, radius);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> BlackWhite(this IAsyncEnumerable<Frame> frames)
    {
        var processor = new BlackWhiteProcessor();
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> Brightness(this IAsyncEnumerable<Frame> frames, float amount)
    {
        var processor = new BrightnessProcessor(amount);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> Contrast(this IAsyncEnumerable<Frame> frames, float amount)
    {
        var processor = new ContrastProcessor(amount);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> Lightness(this IAsyncEnumerable<Frame> frames, float amount)
    {
        var processor = new LightnessProcessor(amount);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> Saturate(this IAsyncEnumerable<Frame> frames, float amount)
    {
        var processor = new SaturateProcessor(amount);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> Opacity(this IAsyncEnumerable<Frame> frames, float amount)
    {
        var processor = new OpacityProcessor(amount);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }

    public static IAsyncEnumerable<Frame> Hue(this IAsyncEnumerable<Frame> frames, float degrees)
    {
        var processor = new HueProcessor(degrees);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }

    public static IAsyncEnumerable<Frame> Invert(this IAsyncEnumerable<Frame> frames, float degrees)
    {
        var processor = new InvertProcessor(degrees);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> Kodachrome(this IAsyncEnumerable<Frame> frames)
    {
        var processor = new KodachromeProcessor();
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    
    private static readonly GraphicsOptions DefaultGraphicsOptions = new GraphicsOptions
    {
        Antialias = true,
    }; 
    public static IAsyncEnumerable<Frame> Polaroid(this IAsyncEnumerable<Frame> frames, GraphicsOptions? graphicsOptions = null)
    {
        var processor = new PolaroidProcessor(graphicsOptions ?? DefaultGraphicsOptions);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }

    public static IAsyncEnumerable<Frame> Pixelate(this IAsyncEnumerable<Frame> frames, int size)
    {
        var processor = new PixelateProcessor(size);
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