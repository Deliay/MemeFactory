using System.Runtime.CompilerServices;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp.Processing;

namespace MemeFactory.Core.Utilities;

public static class LcmExpander
{
    public static SequenceMerger LcmExpand(this IAsyncEnumerable<Frame> b,
        int minimalKeepCount = -1, CancellationToken cancellationToken = default)
    {
        return (a) => ExpandCore(a, b, minimalKeepCount, cancellationToken);
    }
    
    private static async IAsyncEnumerable<(Frame a, Frame b)> ExpandCore(this IAsyncEnumerable<Frame> first,
        IAsyncEnumerable<Frame> second, int secondMinimalKeepCount = -1,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var firstSeq = await first.ToListAsync(cancellationToken);
        var secondSeq = await second.ToListAsync(cancellationToken);

        var minimalKeepCount = secondMinimalKeepCount > 0 ? secondMinimalKeepCount : secondSeq.Count;
        var total = Enumerable.Range(minimalKeepCount, secondSeq.Count)
            .Select(c => Algorithms.Lcm(firstSeq.Count, c))
            .Min();
        
        var shortSeq = firstSeq.Count > secondSeq.Count ? (secondSeq) : (firstSeq);

        var loopTimes = (total / shortSeq.Count) - 1;

        var frames = (firstSeq.Count > secondSeq.Count
            ? firstSeq.Zip(secondSeq.Loop(loopTimes))
            : firstSeq.Loop(loopTimes).Zip(secondSeq)).ToList();
        
        foreach (var tuple in frames)
        {
            yield return tuple;
        }
    }
}