using MemeFactory.Core.Processing;

namespace MemeFactory.Ffmpeg;

public static class FfmpegFilterExtensions
{
    public static IAsyncEnumerable<Frame> SpeedUp(this IAsyncEnumerable<Frame> frames, float timeScale,
        CancellationToken cancellationToken = default)

    {
        return frames.FfmpegToGif(v => v.Arguments.Add(new SetPtsVideoFilter(timeScale)), cancellationToken: cancellationToken);
    }
}
