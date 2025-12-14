using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MemeFactory.Core.Utilities;

public static class Utilities
{
    internal static Frame Scale(this Frame frame, Size size, AnchorPositionMode anchor)
    {
        frame.Image.Mutate(ctx =>
        {
            ctx.Resize(new ResizeOptions()
            {
                Position = anchor,
                Size = size,
                Compand = false,
                Mode = ResizeMode.BoxPad
            });
        });
        return frame;
    }
}