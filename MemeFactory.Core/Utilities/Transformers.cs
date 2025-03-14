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
        int directionHorizontal = 1, int directionVertical = 0, int slidingFrames = 20,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var allFrames = await frames.ToListAsync(cancellationToken);

        var targetFrames = allFrames.Count;
        
        // calculate a LCM number between to make the frames smoooooth
        (targetFrames, slidingFrames) = Enumerable.Range(slidingFrames - 5, 10)
            .Select(c => (Algorithms.Lcm(targetFrames, c), c))
            .MinBy(p => p.Item1);
        var safeRepeatCount = targetFrames / slidingFrames;
        
        var imageSize = allFrames[0].Image.Size;
        // get the distance each frame moved
        var eachX = Convert.ToInt32((1f * imageSize.Width * safeRepeatCount / targetFrames));
        var eachY = Convert.ToInt32((1f * imageSize.Height * safeRepeatCount / targetFrames));
        
        var sequenceExtraLoopTimes = targetFrames / allFrames.Count - 1;
        var safeFrameCount = slidingFrames * safeRepeatCount - 10;
        var finalSequence = allFrames.Loop(sequenceExtraLoopTimes).ToList();
        var currentFrameIndex = 0;
        var slidingGap = slidingFrames / 2;
        for (var i = 0; i < safeFrameCount; i++)
        {
            using var left = finalSequence[i];
            using var right = finalSequence[slidingGap + i];
            Image newFrame = new Image<Rgba32>(imageSize.Width, imageSize.Height);
            newFrame.Mutate(ProcessSlide(currentFrameIndex % slidingFrames, left.Image, right.Image));
            yield return new Frame() { Sequence = currentFrameIndex++, Image = newFrame };
            if ((i + 1) % slidingGap == 0) i += slidingGap;
        }

        if (finalSequence.Count % 2 != 0)
        {
            using var final = finalSequence[^1];
            Image newFrame = new Image<Rgba32>(imageSize.Width, imageSize.Height);
            newFrame.Mutate(ProcessSlide(currentFrameIndex % slidingFrames, null, final.Image));
            yield return new Frame() { Sequence = currentFrameIndex, Image = newFrame };
        }

        yield break;

        Action<IImageProcessingContext> ProcessSlide(int i, Image? left, Image? right)
        {
            return ctx =>
            {
                if (left is not null)
                {
                    var leftX = directionHorizontal != 0 ? 0 - eachX * i : 0;
                    var leftY = directionVertical != 0 ? 0 - eachY * i : 0;
                    var leftPos = new Point(
                        (leftX) * directionHorizontal,
                        (leftY) * directionVertical);
                    
                    ctx.DrawImage(left, leftPos, 1f);
                }

                if (right is not null)
                {
                    var rightX = directionHorizontal != 0 ? imageSize.Width - eachX * i : 0;
                    var rightY = directionVertical != 0 ? imageSize.Height - eachY * i : 0;
                    var rightPos = new Point(
                        (rightX) * directionHorizontal,
                        (rightY) * directionVertical);
                    ctx.DrawImage(right, rightPos, 1f);
                }
            };
        }
    }
}