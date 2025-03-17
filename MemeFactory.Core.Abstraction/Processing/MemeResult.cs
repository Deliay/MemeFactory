using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;

namespace MemeFactory.Core.Processing;

/// <summary>
/// The result, includes the final image object, and it's encoder and mimetype.
/// </summary>
/// <param name="Image"></param>
/// <param name="Encoder"></param>
/// <param name="MimeType"></param>
public readonly record struct MemeResult(Image Image, IImageEncoder Encoder, string MimeType, string Extension) : IDisposable
{
    private static readonly PngEncoder DefaultPngEncoder = new();
    private static readonly GifEncoder DefaultGifEncoder = new();
    
    public static MemeResult Png(Image image) => new(image, DefaultPngEncoder, "image/png", "png");
    public static MemeResult Gif(Image image) => new(image, DefaultGifEncoder, "image/gif", "gif");
    
    public void Dispose()
    {
        Image.Dispose();
    }
}
