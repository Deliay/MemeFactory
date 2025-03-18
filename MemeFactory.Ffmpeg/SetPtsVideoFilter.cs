using FFMpegCore.Arguments;

namespace MemeFactory.Ffmpeg;

public class SetPtsVideoFilter(float scale) : IVideoFilterArgument
{
    public string Key => "[f]setpts";
    public string Value => $"{scale}*PTS";
}