using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Pipes;
using MemeFactory.Core.Processing;
using MemeFactory.Core.Utilities;
using SixLabors.ImageSharp;

namespace MemeFactory.Ffmpeg;

public static class FfmpegExtension
{
    public readonly record struct TempFile(string Path) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }

    public static async ValueTask<TempFile> CreateTempFile(MemeResult memeResult,
        CancellationToken cancellationToken = default)
    {
        var path = Path.GetTempFileName() + "." + memeResult.Extension;
        await memeResult.Image.SaveAsync(path, cancellationToken);
        
        return new TempFile(path);
    }

    private static async ValueTask ProcessByFile(MemeResult result, Stream outputStream,
        Action<FFMpegArgumentOptions> inputOptions,
        Action<FFMpegArgumentOptions> outputOptions,
        FFOptions? ffOptions = null,
        CancellationToken cancellationToken = default)
    {
        using var imageFile = await CreateTempFile(result, cancellationToken);
        
        await FFMpegArguments
            .FromFileInput(imageFile.Path, addArguments: inputOptions)
            .OutputToPipe(new StreamPipeSink(outputStream), outputOptions)
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously(ffMpegOptions: ffOptions);
    }

    private static async ValueTask ProcessByStream(MemeResult result, Stream outputStream,
        Action<FFMpegArgumentOptions> inputOptions,
        Action<FFMpegArgumentOptions> outputOptions,
        FFOptions? ffOptions = null,
        CancellationToken cancellationToken = default)
    {
        await using Stream inputStream = new MemoryStream();
        await result.Image.SaveAsync(inputStream, result.Encoder, cancellationToken);
        
        inputStream.Seek(0, SeekOrigin.Begin);
        await FFMpegArguments
            .FromPipeInput(new StreamPipeSource(inputStream), inputOptions)
            .OutputToPipe(new StreamPipeSink(outputStream), outputOptions)
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously(ffMpegOptions: ffOptions);
    }
    
    public static async IAsyncEnumerable<Frame> Ffmpeg(this IAsyncEnumerable<Frame> frames,
        Action<FFMpegArgumentOptions> inputOptions,
        Action<FFMpegArgumentOptions> outputOptions,
        Func<Stream, IAsyncEnumerable<Frame>> outputResultMapper,
        FFOptions? ffOptions = null,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)

    {
        await using var outputStream = new MemoryStream();
        using var result = await frames.AutoComposeAsync(cancellationToken);
        
        // TODO figure out why pipe input in linux can cause ffmpeg error
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await ProcessByFile(result, outputStream, inputOptions, outputOptions, ffOptions, cancellationToken);
        }
        else
        {
            await ProcessByStream(result, outputStream, inputOptions, outputOptions, ffOptions, cancellationToken);
        }
        
        outputStream.Seek(0, SeekOrigin.Begin);
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