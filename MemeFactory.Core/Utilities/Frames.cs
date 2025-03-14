using System.Runtime.CompilerServices;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemeFactory.Core.Utilities;

public static class Frames
{
    public static Frame Copy(this ImageFrameCollection frameCollection, int sequence)
    {
        var rawFrame = frameCollection[sequence];
        using var frame = frameCollection.CloneFrame(sequence);
        var newImage = new Image<Rgba32>(frame.Width, frame.Height);
        newImage.Mutate(x => x.DrawImage(frame, 1.0f));
        rawFrame.CopyGifPropertiesTo(newImage.Frames.RootFrame);
        return new Frame(sequence, newImage);
    }
    
    private static void CopyGifPropertiesTo(this ImageFrame src, ImageFrame dest)
    {
        src.Metadata.GetGifMetadata().CopyGifPropertiesTo(dest.Metadata.GetGifMetadata());
    }
    
    private static void CopyGifPropertiesTo(this GifFrameMetadata src, GifFrameMetadata dest)
    {
        dest.DisposalMethod = dest.DisposalMethod;
        dest.FrameDelay = src.FrameDelay;
        dest.HasTransparency = src.HasTransparency;
        dest.LocalColorTable = src.LocalColorTable;
        dest.TransparencyIndex = src.TransparencyIndex;
        dest.ColorTableMode = src.ColorTableMode;
    }

    public static IAsyncEnumerable<Frame> ExtractFrames(this Image src)
    {
        return Enumerable.Range(0, src.Frames.Count)
            .Select(sequence => src.Frames.Copy(sequence))
            .ToAsyncEnumerable();
    }
    
    public static IAsyncEnumerable<Frame> EachFrame(this IAsyncEnumerable<Frame> frames,
        FrameProcessor processor, CancellationToken cancellationToken = default)
    {
        return frames
            .SelectAwait((f) => processor(f, cancellationToken))
            .OrderBy(f => f.Sequence);
    }

    public static IAsyncEnumerable<Frame> FrameBasedZipSequence(this IAsyncEnumerable<Frame> meme,
        SequenceMerger sequenceMerger, FrameMerger frameMerger, CancellationToken cancellationToken = default)
    {
        return sequenceMerger(meme).SelectAwait(async tuple =>
        {
            using var disposeB = tuple.b;
            return await frameMerger(tuple.a, disposeB, cancellationToken);
        });
    }

    public static async IAsyncEnumerable<Frame> DuplicateFrame(this IAsyncEnumerable<Frame> src, int times)
    {
        var index = 0;
        await foreach (var frame in src)
        {
            yield return frame with { Sequence = index++ };
            for (var i = 0; i < times; i++)
            {
                yield return new Frame(Sequence: index++, Image: frame.Image.Clone((_) => {}));
            }
        }
    }
    
    public static async ValueTask<Sequence> ToSequenceAsync(this IAsyncEnumerable<Frame> frames,
        CancellationToken cancellationToken = default)
    {
        return new Sequence(await frames.ToListAsync(cancellationToken));
    }

    public static async IAsyncEnumerable<Frame> LoadFromFolderAsync(string path, string pattern = "*.png",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var enumerateFiles = Directory
            .EnumerateFiles(path, pattern)
            .Order();

        var fileIndex = 1;
        foreach (var file in enumerateFiles)
        {
            yield return Frame.Of(fileIndex++, await Image.LoadAsync(file, cancellationToken));
        }
    }

    public static IEnumerable<Frame> Loop(this IEnumerable<Frame> source, int times = 999)
    {
        List<Frame> cache = [];
        var index = 0;
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
}
