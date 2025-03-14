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
        int directionHorizontal = 1, int directionVertical = 0, int totalMoves = 20,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var allFrames = await frames.ToListAsync(cancellationToken);

        // padding to more than 20 frames when not enough
        var targetFrames = allFrames.Count;
        if (targetFrames < totalMoves)
        {
            targetFrames = Convert.ToInt32(Math.Ceiling(1f * totalMoves / targetFrames)) * targetFrames;
        }
        var imageSize = allFrames[0].Image.Size;
        
        var eachX = Convert.ToInt32((1f * imageSize.Width / targetFrames));
        var eachY = Convert.ToInt32((1f * imageSize.Height / targetFrames));
        var loopTimes = targetFrames / allFrames.Count;

        var finalFrames = allFrames.Loop(loopTimes).ToList();
        var halfFrameIndex = finalFrames.Count / 2;
        for (var i = 0; i < halfFrameIndex; i++)
        {
            using var left = finalFrames[i];
            using var right = finalFrames[halfFrameIndex + i];
            Image newFrame = new Image<Rgba32>(imageSize.Width, imageSize.Height);
            newFrame.Mutate(ProcessSlide(i + 1, left.Image, right.Image));
            yield return new Frame() { Sequence = i, Image = newFrame };
        }

        if (finalFrames.Count % 2 != 0)
        {
            yield return finalFrames[^1];
        }

        yield break;

        Action<IImageProcessingContext> ProcessSlide(int i, Image left, Image right)
        {
            return ctx =>
            {
                var leftX = directionHorizontal != 0 ? 0 - eachX * i : 0;
                var leftY = directionVertical != 0 ? 0 - eachY * i : 0;
                
                var rightX = directionHorizontal != 0 ? imageSize.Width - eachX * i : 0;
                var rightY = directionVertical != 0 ? imageSize.Height - eachY * i : 0;
                
                ctx.DrawImage(left, new Point(leftX * directionHorizontal, leftY * directionVertical), 1f);
                ctx.DrawImage(right, new Point(rightX * directionHorizontal, rightY * directionVertical), 1f);
            };
        }
    }
}