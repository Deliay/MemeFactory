using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;

namespace MemeFactory.Core.Utilities;

public static class Resizer
{
    public static Func<Frame, Size> Auto(this Image image)
    {
        return (frame) =>
        {
            var aimSize = frame.Image.Size;
            var targetSize = image.Size;
            targetSize = new Size(aimSize.Width, Convert.ToInt32(1f * aimSize.Width / targetSize.Width * targetSize.Height));
            targetSize = new Size(Convert.ToInt32(1f * aimSize.Height / targetSize.Height * targetSize.Width), aimSize.Height);
            
            return targetSize;
        };
    }
}