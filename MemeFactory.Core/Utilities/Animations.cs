using System.Numerics;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemeFactory.Core.Utilities;

public static class Animations
{
    public static Frame Projective(this Frame frame, Action<ProjectiveTransformBuilder> builder)
    {
        return frame.Mutate(ctx =>
        {
            var projectiveTransformBuilder = new ProjectiveTransformBuilder();
            builder(projectiveTransformBuilder);
            ctx.Transform(projectiveTransformBuilder);
        });
    }

    public static Frame Resize(this Frame frame, Size size,
        AnchorPositionMode anchor = AnchorPositionMode.Center,
        ResizeMode mode = ResizeMode.BoxPad)
    {
        return frame.Mutate(ctx => ctx.Resize(new ResizeOptions()
        {
            Size = size,
            Mode = mode,
            Position = anchor,
        }));
    }
    

    public static Frame Scale(this Frame frame, SizeF sizef, AnchorPositionMode anchor)
    {
        return frame.Mutate(ctx =>
        {
            var size = ctx.GetCurrentSize();
            ctx.Resize(new ResizeOptions()
            {
                Mode = ResizeMode.BoxPad,
                Position = anchor,
                PremultiplyAlpha = true,
                TargetRectangle = new Rectangle(Point.Empty, size),
                Size = new Size((int)(size.Width * sizef.Width), (int)(size.Height * sizef.Height)),
            });
        });
    }

    public static IAsyncEnumerable<Frame> Scale(this IAsyncEnumerable<Frame> frames, SizeF sizef,
        AnchorPositionMode anchor)
    {
        return frames.Select(f => f.Scale(sizef, anchor));
    }

    public static Frame Mutate(this Frame frame, Action<IImageProcessingContext> ctx)
    {
        frame.Image.Mutate(ctx);
        return frame;
    }

    public static Frame ResizePixelOnly(this Frame f, SizeF size, AnchorPositionMode anchor)
    {
        var oldSize = f.Image.Size;
        f.Scale(size, AnchorPositionMode.Top);
        f.Resize(oldSize, AnchorPositionMode.Top);
        return f;
    }
    
    public static IEnumerable<Func<Frame, Frame>> JumpingCore()
    {
        yield return f => f;
        yield return f => f.Projective(b => b.AppendScale(new SizeF(1.1f, 0.9f)));
        yield return f => f.Projective(b => b.AppendScale(new SizeF(1.2f, 0.8f)));
        yield return f => f.Projective(b => b.AppendScale(new SizeF(1.3f, 0.7f)));
        yield return f => f.Projective(b => b.AppendScale(new SizeF(1.2f, 0.8f)));
        yield return f => f.Projective(b => b.AppendScale(new SizeF(1.7f, 1.3f)));
        yield return f => f.ResizePixelOnly(new SizeF(1.2f, 1.4f), AnchorPositionMode.TopLeft);
        yield return f => f.ResizePixelOnly(new SizeF(1.0f, 1.2f), AnchorPositionMode.TopLeft);
        yield return f => f.Projective(b => b.AppendScale(new SizeF(1.3f, 0.7f)));
        yield return f => f.Projective(b => b.AppendScale(new SizeF(1.2f, 0.8f)));
        yield return f => f.Projective(b => b.AppendScale(new SizeF(1.1f, 0.9f)));
        yield return f => f;
    }

    private static IAsyncEnumerable<Frame> ApplyTransformsWithLoop(this IAsyncEnumerable<Frame> frames,
        IEnumerable<Func<Frame, Frame>> transformers)
    {
        var list = transformers.ToAsyncEnumerable();

        return list.LcmExpand()(frames)
            .Select((tuple => tuple.b(tuple.a)));
    }
    
    public static async IAsyncEnumerable<Frame> Jumping(this IAsyncEnumerable<Frame> frames)
    {
        var framesList = await frames.ToListAsync();
        var size = framesList.First().Image.Size;
        await foreach (var frame in framesList.ToAsyncEnumerable()
                           .Scale(new SizeF(1.2f, 1.2f), AnchorPositionMode.Bottom)
                           .ApplyTransformsWithLoop(JumpingCore())
                           .Select(f => f.Resize(size, AnchorPositionMode.Bottom)))
        {
            yield return frame;
        }
    }
}