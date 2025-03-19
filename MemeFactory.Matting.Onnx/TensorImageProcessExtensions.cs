using MemeFactory.Matting.Onnx.Models;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace MemeFactory.Matting.Onnx;

public static class TensorImageProcessExtensions
{
    public static Tensor<float> NormalizeImageToTensor(this Image<Rgba32> image, ModelConfiguration model)
    {
        using var copiedImage = image.Clone(c => c.Resize(model.InputWidth, model.InputHeight,
            new BicubicResampler()));

        var tensor = new DenseTensor<float>([1, 3, model.InputHeight, model.InputWidth]);
        
        Parallel.For(0, copiedImage.Height, (int y) =>
        {
            for (var x = 0; x < model.InputWidth; x++)
            {
                var pixel = copiedImage[x, y];
                tensor[[0, 0, y, x]] = model.NormalizeRed(pixel.R);
                tensor[[0, 1, y, x]] = model.NormalizeGreen(pixel.G);
                tensor[[0, 2, y, x]] = model.NormalizeBlue(pixel.B);
            }
        });
        return tensor;
    }

    public static Image<Rgba32> ConvertTensorToImageMask(this Tensor<float> tensor, ModelConfiguration model, Image src)
    {
        var mask = new Image<Rgba32>(model.InputWidth, model.InputHeight);

        Parallel.For(0, model.InputHeight, (int y) =>
        {
            for (var x = 0; x < model.InputWidth; x++)
            {
                var sigmoidValue = CalculateSigmoid(tensor[[0, 0, y, x]]);
                var normalizedValue = Normalize(sigmoidValue);
                var intensity = ConvertToGreyscale(normalizedValue);
                
                mask[x, y] = new Rgba32(intensity, intensity, intensity, 255);
            }
        });
        mask.Mutate(x => x.Resize(src.Width, src.Height, new BicubicResampler()));
        return mask;
    }

    public static Image<Rgba32> ApplyMaskToImage(this Image<Rgba32> src, Image<Rgba32> mask, int confidence = 20)
    {
        var finalImage = new Image<Rgba32>(src.Width, src.Height);
        var transparentPixel = new Rgba32(0, 0, 0, 0);

        Parallel.For(0, src.Height, (int y) =>
        {
            for (var x = 0; x < src.Width; x++)
            {
                var srcPixel = src[x, y];
                var maskPixel = mask[x, y];
                var alpha = maskPixel.R;
                
                finalImage[x, y] = alpha > confidence
                    ? new Rgba32(srcPixel.R, srcPixel.G, srcPixel.B, srcPixel.A)
                    : transparentPixel;
            }
        });

        return finalImage;
    }

    private static float Normalize(float value)
    {
        const float binarizationThreshold = 0.5f;
        const float normalizationFactor = 2f;

        return value > binarizationThreshold
            ? (value - binarizationThreshold) * normalizationFactor
            : 0f;
    }

    private static byte ConvertToGreyscale(float value)
    {
        const float maxIntensity = 255f;
        return (byte)(value * maxIntensity);
    }

    private static float CalculateSigmoid(float x)
    {
        const float sigmoidScale = 1f;
        const float sigmoidShift = 1f;
        const float sigmoidDivisor = -1f;

        return sigmoidScale / (sigmoidShift + MathF.Exp(sigmoidDivisor * x));
    }
}