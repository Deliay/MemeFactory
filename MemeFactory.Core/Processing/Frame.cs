using SixLabors.ImageSharp;

namespace MemeFactory.Core.Processing;

public readonly record struct Frame(int Sequence, Image Image) : IDisposable
{
    public static Frame Single(Image image) => new(0, image);
    
    public static Frame Of(int sequence, Image image) => new(sequence, image);
    
    public void Dispose()
    {
        Image.Dispose();
    }
}