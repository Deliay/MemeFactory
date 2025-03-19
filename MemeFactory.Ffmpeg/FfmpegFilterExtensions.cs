using FFMpegCore;
using MemeFactory.Core.Processing;

namespace MemeFactory.Ffmpeg;

public static class FfmpegFilterExtensions
{
    public static IAsyncEnumerable<Frame> SpeedUp(this IAsyncEnumerable<Frame> frames, float timeScale,
        FFOptions? ffOptions = null,
        CancellationToken cancellationToken = default)

    {
        return frames.FfmpegToGif(v => v.Arguments.Add(new SetPtsVideoFilter(timeScale)),
            null, null, ffOptions, cancellationToken);
    }
}
