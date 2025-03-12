using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing;

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
                using var newImage = image.Clone(ctx => ctx.Resize(sizer(f)));
                var poser = imagePos(newImage);
                frameCtx.DrawImage(newImage, poser(f), 1.0f);
            });
            return ValueTask.FromResult(f);
        };
    }

    private static void SetupGifMetadata(this ImageFrame frame)
    {
        var gifFrameMetadata = frame.Metadata.GetGifMetadata();
        gifFrameMetadata.FrameDelay = 0;
        gifFrameMetadata.HasTransparency = false;
        gifFrameMetadata.DisposalMethod = GifDisposalMethod.RestoreToBackground;
    }
    
    public static async ValueTask<MemeResult> AutoComposeAsync(this IAsyncEnumerable<Frame> frames,
        CancellationToken cancellationToken = default)
    {
        var proceedFrames = await frames.ToListAsync(cancellationToken);

        if (proceedFrames.Count == 0) throw new ArgumentException("Invalid sequence count", nameof(frames));
        
        if (proceedFrames.Count == 1) return MemeResult.Png(proceedFrames[0].Image.Clone(_ => {}));
        
        using var rootFrame = proceedFrames[0];
        using var clonedFrame = rootFrame.Image.Frames.CloneFrame(0);
        var templateImage = clonedFrame.Clone(_ => { });

        var rootMetadata = templateImage.Metadata.GetGifMetadata();
        rootMetadata.RepeatCount = 0;
        
        templateImage.Frames.RootFrame.SetupGifMetadata();
        foreach (var (index, proceedImage) in proceedFrames[1..]) using (proceedImage)
        {
            templateImage.Frames.InsertFrame(index, proceedImage.Frames.RootFrame);
            templateImage.Frames[index].SetupGifMetadata();
        }

        return MemeResult.Gif(templateImage);
    }
}