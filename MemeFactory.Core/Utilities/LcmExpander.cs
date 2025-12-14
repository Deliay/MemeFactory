using System.Runtime.CompilerServices;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp.Processing;

namespace MemeFactory.Core.Utilities;

public static class LcmExpander
{
    public static SequenceMerger LcmExpand(this IAsyncEnumerable<Frame> b,
        int minimalKeepCount = -1, CancellationToken cancellationToken = default)
    {
        return (a) => Zip(a, b, minimalKeepCount, cancellationToken);
    }
    public static GenericSequenceMerger<T> LcmExpand<T>(this IAsyncEnumerable<T> b,
        int minimalKeepCount = -1, CancellationToken cancellationToken = default)
    {
        return (a) => Zip(a, b, minimalKeepCount, cancellationToken);
    }
    
    private static async IAsyncEnumerable<(Frame a, Frame b)> Zip(this IAsyncEnumerable<Frame> first,
        IAsyncEnumerable<Frame> second, int secondMinimalKeepCount = -1,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var firstSeq = await first.ToListAsync(cancellationToken);
        var secondSeq = await second.ToListAsync(cancellationToken);

        var loopTimes = LoopTimes(firstSeq.Count, secondSeq.Count, secondMinimalKeepCount);

        var frames = (firstSeq.Count > secondSeq.Count
            ? firstSeq.Zip(secondSeq.Loop(loopTimes))
            : firstSeq.Loop(loopTimes).Zip(secondSeq)).ToList();
        
        foreach (var tuple in frames)
        {
            yield return tuple;
        }
    }

    private static async IAsyncEnumerable<(Frame a, T b)> Zip<T>(this IAsyncEnumerable<Frame> first,
        IAsyncEnumerable<T> second, int secondMinimalKeepCount = -1,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var firstSeq = await first.ToListAsync(cancellationToken);
        var secondSeq = await second.ToListAsync(cancellationToken);

        var loopTimes = LoopTimes(firstSeq.Count, secondSeq.Count, secondMinimalKeepCount);

        var frames = (firstSeq.Count > secondSeq.Count
            ? firstSeq.Zip(secondSeq.Loop((a) => a, loopTimes))
            : firstSeq.Loop(loopTimes).Zip(secondSeq)).ToList();
        
        foreach (var tuple in frames)
        {
            yield return tuple;
        }
    }

    private static int LoopTimes(int firstCount, int secondCount, int secondMinimalKeepCount)
    {
        var minimalKeepCount = secondMinimalKeepCount > 0 ? secondMinimalKeepCount : secondCount;
        var total = Enumerable.Range(minimalKeepCount, secondCount)
            .Select(c => Algorithms.Lcm(firstCount, c))
            .Min();
        
        var shortSeq = firstCount > secondCount ? secondCount : firstCount;

        return (total / shortSeq) - 1;
    }
}