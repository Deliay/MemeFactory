using System.Runtime.CompilerServices;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

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
            frame.Image.Mutate((ctx) => ctx.Rotate(deg * frame.Sequence - 1, new BicubicResampler()));
            var newFrame = new Image<Rgba32>(baseSize.Width, baseSize.Height);
            var x = (baseSize.Width - frame.Image.Width) / 2;
            var y = (baseSize.Height - frame.Image.Height) / 2;
            newFrame.Mutate((ctx) => ctx.DrawImage(frame.Image, new Point(x, y), 1f));
            yield return frame with { Image = newFrame.WithGifMetadata(frame) };
        }
    }

    public static async IAsyncEnumerable<Frame> Sliding(this IAsyncEnumerable<Frame> frames,
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
            yield return new Frame { Sequence = i, Image = newFrame.WithGifMetadata(frame) };
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
    
    public static async IAsyncEnumerable<Frame> TimelineSliding(this IAsyncEnumerable<Frame> frames,
        int directionHorizontal = 1, int directionVertical = 0, int slidingFrames = 8,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var allFrames = await frames.ToListAsync(cancellationToken);

        var targetFrames = allFrames.Count;

        // calculate LCM between slidingFrames and targetFrames
        // to make the frames smoooooth
        (targetFrames, slidingFrames) = Enumerable.Range(slidingFrames - (slidingFrames / 4), slidingFrames / 2)
            .Select(c => (Algorithms.Lcm(targetFrames, c), c))
            .MinBy(p => p.Item1);
        
        while (slidingFrames * 2 >= targetFrames)targetFrames += allFrames.Count;
        if ((targetFrames / slidingFrames) % 2 != 0) targetFrames += allFrames.Count;
        
        var imageSize = allFrames[0].Image.Size;
        // get the distance each frame moved
        var eachX = (int)Math.Round(1f * imageSize.Width / slidingFrames);
        var eachY = (int)Math.Round(1f * imageSize.Height / slidingFrames);
        
        var slidingGap = slidingFrames / 2;
        var safeFrameCount = targetFrames - slidingGap;
        
        var sequenceExtraLoopTimes = targetFrames / allFrames.Count;
        var finalSequence = allFrames.Loop(sequenceExtraLoopTimes - 1).ToList();
        
        var currentFrameIndex = 0;
        for (var i = 0; i < safeFrameCount; i++)
        {
            using var left = finalSequence[i];
            using var right = finalSequence[slidingGap + i];
            Image newFrame = new Image<Rgba32>(imageSize.Width, imageSize.Height);
            newFrame.Mutate(ProcessSlide(currentFrameIndex % slidingFrames, left.Image, right.Image));
            yield return new Frame { Sequence = currentFrameIndex++, Image = newFrame.WithGifMetadata(right) };
            if ((i + 1) % slidingGap == 0) i += slidingGap;
        }

        yield break;

        Action<IImageProcessingContext> ProcessSlide(int i, Image left, Image right)
        {
            return ctx =>
            {
                var baseX = eachX * i;
                var baseY = eachY * i;
                
                var leftX = directionHorizontal * (0 - baseX);
                var rightX = directionHorizontal * (imageSize.Width - baseX);
                
                var leftY = directionVertical * (0 - baseY);
                var rightY = directionVertical * (imageSize.Height - baseY);
                
                var leftPos = new Point(leftX, leftY);
                var rightPos = new Point(rightX, rightY);
                
                ctx.DrawImage(left, leftPos, 1f);
                ctx.DrawImage(right, rightPos, 1f);

            };
        }
    }
    
    public static IAsyncEnumerable<Frame> Flip(this IAsyncEnumerable<Frame> frames, FlipMode flipMode)
    {
        var processor = new FlipProcessor(flipMode);
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(processor);
            });
            return f;
        });
    }
    public static IAsyncEnumerable<Frame> Resize(this IAsyncEnumerable<Frame> frames, ResizeOptions options)
    {
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx =>
            {
                ctx.ApplyProcessor(new ResizeProcessor(options, f.Image.Size));
            });
            return f;
        });
    }

    public static IAsyncEnumerable<Frame> ProjectiveTransform(this IAsyncEnumerable<Frame> frames,
        ProjectiveTransformBuilder transformBuilder)

    {
        return frames.Select(f =>
        {
            f.Image.Mutate(ctx => ctx.Transform(transformBuilder));
            return f;
        });
    }
    
    public static IAsyncEnumerable<Frame> Tile(this IAsyncEnumerable<Frame> seq, (int x, int y) tileSize)
    {
        return seq.Select(f =>
        {
            var canvasWidth = f.Image.Width * Math.Min(tileSize.x, 2);
            var canvasHeight = f.Image.Height * Math.Min(tileSize.y, 2);

            var preImageSize = new Size(canvasWidth / tileSize.x, canvasHeight / tileSize.y);

            var canvas = new Image<Rgba32>(canvasWidth, canvasHeight);

            f.Image.Mutate(x => x.Resize(new ResizeOptions()
            {
                Size = preImageSize,
                Sampler = new BicubicResampler(),
            }));

            canvas.Mutate(x =>
            {
                for (var w = 0; w < tileSize.x; w++)
                {
                    for (var h = 0; h < tileSize.y; h++)
                    {
                        var point = new Point(w * preImageSize.Width, h * preImageSize.Height);

                        x.DrawImage(f.Image, point, 1.0f);
                    }
                }
            });

            using var oldFrame = f;
            return oldFrame with { Image = canvas };
        });
    }
}
