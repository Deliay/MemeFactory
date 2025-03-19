using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace MemeFactory.Core.Utilities;

public static class Composers
{
    public static FrameMerger Draw(Func<Image, Func<Frame, Size>> frameSizer,
        Func<Image, Func<Frame, Point>> framePos)
    {
        return (a, b, c) => Draw(b.Image, frameSizer, framePos)(a, c);
    }

    
    public static FrameProcessor Draw(Image image,
        Func<Image, Func<Frame, Size>> imageSizer, Func<Image, Func<Frame, Point>> imagePos)
    {
        var sizer = imageSizer(image);
        return (f, c) =>
        {
            f.Image.Mutate(frameCtx =>
            {
                using var newImage = image.Clone(ctx => ctx.Resize(new ResizeOptions()
                {
                    Size = sizer(f),
                    Mode = ResizeMode.Stretch,
                    Sampler = new BicubicResampler(),
                }));
                var poser = imagePos(newImage);
                frameCtx.DrawImage(newImage, poser(f), 1.0f);
            });
            return ValueTask.FromResult(f);
        };
    }

    public static Image WithGifMetadata(this Image image, Frame template, int overrideFrameDelay = -1)
    {
        image.Frames.RootFrame.WithGifMetadata(template, overrideFrameDelay);
        return image;
    }

    public static void WithGifMetadata(this ImageFrame frame, Frame template, int overrideFrameDelay = -1)
    {
        var gifFrameMetadata = frame.Metadata.GetGifMetadata();
        var templateMetadata = template.Image.Frames.RootFrame.Metadata.GetGifMetadata();
        
        gifFrameMetadata.FrameDelay = overrideFrameDelay > -1
            ? overrideFrameDelay
            : Math.Max(2, templateMetadata.FrameDelay);
        
        gifFrameMetadata.HasTransparency = false;
        gifFrameMetadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;
    }

    public static ValueTask<MemeResult> AutoComposeAsync(this IAsyncEnumerable<Frame> frames,
        CancellationToken cancellationToken = default) =>
        AutoComposeAsync(frames, -1, cancellationToken);
    
    public static async ValueTask<MemeResult> AutoComposeAsync(this IAsyncEnumerable<Frame> frames,
        int overrideFrameDelay,
        CancellationToken cancellationToken = default)
    {
        var proceedFrames = await frames.OrderBy(f => f.Sequence).ToListAsync(cancellationToken);

        switch (proceedFrames.Count)
        {
            case 0:
                throw new ArgumentException("Invalid sequence count", nameof(frames));
            case 1:
                return MemeResult.Png(proceedFrames[0].Image.Clone(_ => {}));
        }

        using var rootFrame = proceedFrames[0];
        var templateImage = rootFrame.Image.Clone(_ => {});

        var rootMetadata = templateImage.Metadata.GetGifMetadata();
        rootMetadata.RepeatCount = 0;
        
        templateImage.WithGifMetadata(rootFrame, overrideFrameDelay);
        foreach (var frame in proceedFrames[1..]) using (frame)
        {
            templateImage.Frames.InsertFrame(frame.Sequence, frame.Image.Frames.RootFrame);
            templateImage.Frames[frame.Sequence].WithGifMetadata(frame, overrideFrameDelay);
        }

        return MemeResult.Gif(templateImage);
    }
    
    public static IAsyncEnumerable<Frame> FrameDelay(this IAsyncEnumerable<Frame> frames, TimeSpan duration)
    {
        return frames.Select(f =>
        {
            var gifFrameMetadata = f.Image.Frames.RootFrame.Metadata.GetGifMetadata();
            gifFrameMetadata.FrameDelay = (int)Math.Round(duration.TotalMilliseconds / 10, MidpointRounding.AwayFromZero);
            
            return f;
        });
    }
}