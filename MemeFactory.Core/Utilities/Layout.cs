using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;

namespace MemeFactory.Core.Utilities;

public static class Layout
{
    public static Func<Frame, Point> LeftTop(this Image image)
    {
        return (frame) => new Point(0, 0);
    }
    public static Func<Frame, Point> LeftCenter(this Image image)
    {
        return (frame) => new Point(0, (frame.Image.Height - image.Height) / 2);
    }
    public static Func<Frame, Point> LeftBottom(this Image image)
    {
        return (frame) => new Point(0, frame.Image.Height - image.Height);
    }
    
    public static Func<Frame, Point> RightTop(this Image image)
    {
        return (frame) => new Point(frame.Image.Width - image.Width, 0);
    }
    public static Func<Frame, Point> RightCenter(this Image image)
    {
        return (frame) => new Point(frame.Image.Width - image.Width,
            (frame.Image.Height - image.Height) / 2);
    }
    public static Func<Frame, Point> RightBottom(this Image image)
    {
        return (frame) => new Point(frame.Image.Width - image.Width, frame.Image.Height - image.Height);
    }

    public static Func<Frame, Point> TopCenter(this Image image)
    {
        return (frame) => new Point((frame.Image.Width - image.Width) / 2, 0);
    }
    public static Func<Frame, Point> Center(this Image image)
    {
        return (frame) => new Point((frame.Image.Width - image.Width) / 2,
            (frame.Image.Height - image.Height) / 2);
    }
    public static Func<Frame, Point> BottomCenter(this Image image)
    {
        return (frame) => new Point(frame.Image.Width - image.Width, (frame.Image.Height - image.Height) / 2);
    }
}