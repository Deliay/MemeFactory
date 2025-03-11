using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;

namespace MemeFactory.Core.Utilities;

public static class Layout
{
    public static Func<Frame, Point> LeftBottom(this Image image)
    {
        return (frame) => new Point(0, frame.Image.Height - image.Height);
    }
    public static Func<Frame, Point> RightCenter(this Image image)
    {
        return (frame) => new Point(frame.Image.Width - image.Width,
            (frame.Image.Height - image.Height) / 2);
    }
}