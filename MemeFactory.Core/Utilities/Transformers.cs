using System.Runtime.CompilerServices;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace MemeFactory.Core.Utilities;

public static class Transformers
{
    public static async IAsyncEnumerable<Frame> Rotation(this IAsyncEnumerable<Frame> frames, int circleTimes = 16,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)

    {
        float deg;
        var total = 0;
        var allFrames = await frames.ToListAsync(cancellationToken);
        if (allFrames.Count > circleTimes)
        {
            deg = (allFrames.Count * 1f / circleTimes) * 360f / allFrames.Count;
        }
        else
        {
            (total, circleTimes) = Enumerable.Range(circleTimes - 2, 4)
                .Select(c => (Algorithms.Lcm(allFrames.Count, c) / allFrames.Count, c))
                .MinBy(p => p.Item1);
            deg = 360f / circleTimes;
            total -= 1;
        }

        var baseSize = allFrames[0].Image.Size;
        foreach (var frame in allFrames.Loop(total).ToList()) using (frame)
        {
            frame.Image.Mutate((ctx) => ctx.Rotate(deg * frame.Sequence - 1));
            var newFrame = new Image<Rgba32>(baseSize.Width, baseSize.Height);
            var x = (baseSize.Width - frame.Image.Width) / 2;
            var y = (baseSize.Height - frame.Image.Height) / 2;
            newFrame.Mutate((ctx) => ctx.DrawImage(frame.Image, new Point(x, y), 1f));
            yield return frame with { Image = newFrame };
        }
    }

    public static async IAsyncEnumerable<Frame> Slide(this IAsyncEnumerable<Frame> frames,
        int directionHorizontal = 1, int directionVertical = 0, int minMoves = 4,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var allFrames = await frames.ToListAsync(cancellationToken);

        // padding to more than `minMoves` frames when not enough
        var targetFrames = allFrames.Count;
        var loopTimes = (minMoves + targetFrames - 1) / targetFrames;

        var finalFrames = allFrames.Loop(loopTimes - 1).ToList();
        for (var i = 0; i < finalFrames.Count; i++)
        {
            using var frame = finalFrames[i];
            Image newFrame = new Image<Rgba32>(frame.Image.Size.Width, frame.Image.Size.Height);
            newFrame.Mutate(ProcessSlide(i, frame.Image));
            yield return new Frame { Sequence = i, Image = newFrame };
        }

        yield break;

        Action<IImageProcessingContext> ProcessSlide(int i, Image image)
        {
            return ctx =>
            {
                var x = (int)Math.Round(1f * i / finalFrames.Count * image.Size.Width, MidpointRounding.AwayFromZero);
                var y = (int)Math.Round(1f * i / finalFrames.Count * image.Size.Height, MidpointRounding.AwayFromZero);

                var leftPos = new Point((x - image.Size.Width) * directionHorizontal, (y - image.Size.Height) * directionVertical);
                var rightPos = new Point(x * directionHorizontal, y * directionVertical);

                ctx.DrawImage(image, leftPos, 1f);
                ctx.DrawImage(image, rightPos, 1f);
            };
        }
    }
}