using System.Runtime.CompilerServices;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Pipes;
using MemeFactory.Core.Processing;
using MemeFactory.Core.Utilities;
using SixLabors.ImageSharp;

namespace MemeFactory.Ffmpeg;

public static class FfmpegExtension
{
    public static async IAsyncEnumerable<Frame> Ffmpeg(this IAsyncEnumerable<Frame> frames,
        Action<FFMpegArgumentOptions> inputOptions,
        Action<FFMpegArgumentOptions> outputOptions,
        Func<MemoryStream, IAsyncEnumerable<Frame>> outputResultMapper,
        FFOptions? ffOptions = null,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)

    {
        using var inputStream = new MemoryStream();
        using var outputStream = new MemoryStream();
        
        using var gif = await frames.AutoComposeAsync(cancellationToken);
        await gif.Image.SaveAsync(inputStream, gif.Encoder, cancellationToken);
        inputStream.Position = 0;

        await FFMpegArguments
            .FromPipeInput(new StreamPipeSource(inputStream), inputOptions)
            .OutputToPipe(new StreamPipeSink(outputStream), outputOptions)
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously(ffMpegOptions: ffOptions);
        
        outputStream.Position = 0;
        await foreach (var frame in outputResultMapper(outputStream))
        {
            yield return frame;
        }
    }
    
    
    public static IAsyncEnumerable<Frame> FfmpegToGif(this IAsyncEnumerable<Frame> frames,
        Action<VideoFilterOptions>? vfOptions = null, Action<FFMpegArgumentOptions>? outputOptions = null,
        Action<FFMpegArgumentOptions>? inputOptions = null,
        FFOptions? ffOptions = null,
        CancellationToken cancellationToken = default)
    {
        return frames.Ffmpeg(input =>
        {
            inputOptions?.Invoke(input);
        }, output =>
        {
            outputOptions?.Invoke(output);
            
            var vfArgObject = new VideoFiltersArgument(new VideoFilterOptions());
            
            var vfArgDefault = "-vf \"split[s1][s2];[s1]palettegen=max_colors=256[p];[s2][p]paletteuse=dither=bayer[f];";
            vfOptions?.Invoke(vfArgObject.Options);
            vfArgDefault = vfArgObject.Options.Arguments
                .Aggregate(vfArgDefault, (current, vfItem) => current + (vfItem.Key + '=' + vfItem.Value));
            
            output.WithCustomArgument(vfArgDefault + "\"");
            output.WithFramerate(24);
            output.ForceFormat("gif");
        }, stream => MapToSequence(stream, cancellationToken), ffOptions, cancellationToken);
    }

    private static async IAsyncEnumerable<Frame> MapToSequence(Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)

    {
        using var image = await Image.LoadAsync(stream, cancellationToken);
        await foreach (var extractFrame in image.ExtractFrames().WithCancellation(cancellationToken))
        {
            yield return extractFrame;
        }
    }
}