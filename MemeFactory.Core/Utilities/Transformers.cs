using System.Runtime.CompilerServices;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp.Processing;

namespace MemeFactory.Core.Utilities;

public static class Transformers
{
    public static async IAsyncEnumerable<Frame> Rotation(this IAsyncEnumerable<Frame> frames, int circleTimes = 16,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)

    {
        var deg = 360f / circleTimes;
        var allFrames = await frames.ToListAsync(cancellationToken);
        var total = Algorithms.Lcm(allFrames.Count, circleTimes) / allFrames.Count - 1;

        foreach (var frame in allFrames.Loop(total))
        {
            frame.Image.Mutate((ctx) => ctx.Rotate(deg * frame.Sequence));
            yield return frame;
        }
    }
}