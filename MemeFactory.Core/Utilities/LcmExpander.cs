using System.Runtime.CompilerServices;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp.Processing;

namespace MemeFactory.Core.Utilities;

public static class LcmExpander
{    
    private static IEnumerable<Frame> Loop(this IEnumerable<Frame> source, int times = 999)
    {
        List<Frame> cache = [];
        var index = 1;
        foreach (var item in source)
        {
            cache.Add(item);
            yield return item with { Sequence = index++ };
        }

        var currentTimes = 0;
        while (currentTimes++ < times)
        {
            foreach (var item in cache)
            {
                yield return new Frame(index++, item.Image.Clone((_) => {}));
            }
        }
    }
    private static int Gcd(int a, int b) {
        while (b != 0) {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
    private static int Lcm(int a, int b) {
        var gcd = Gcd(a, b);
        return (a * b) / gcd;
    }

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
            .Select(c => Lcm(firstSeq.Count, c))
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