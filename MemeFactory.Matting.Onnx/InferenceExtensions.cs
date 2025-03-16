using MemeFactory.Core.Processing;
using MemeFactory.Matting.Onnx.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp.PixelFormats;

namespace MemeFactory.Matting.Onnx;

public static class InferenceExtensions
{
    public static InferenceSession GetInferenceSession(this ModelConfiguration modelConfiguration)
    {
        return new InferenceSession(modelConfiguration.ModelPath);
    }
    
    public static Tensor<float> Inference(this InferenceSession session, Tensor<float> input)
    {
        var inputs = new List<NamedOnnxValue>()
        {
            NamedOnnxValue.CreateFromTensor(session.InputNames[0], input),
        };
        
        using var results = session.Run(inputs);
        return results[0].AsTensor<float>().Clone();
    }

    public static async IAsyncEnumerable<Frame> ApplyModel(this IAsyncEnumerable<Frame> frames, ModelConfiguration model)
    {
        using var session = model.GetInferenceSession();
        await foreach (var frame in frames)
        {
            using var currentFrame = frame;
            using var src = frame.Image.CloneAs<Rgba32>();

            var input = src.NormalizeImageToTensor(model);
            var output = session.Inference(input);

            using var mask = output.ConvertTensorToImageMask(model, src);

            yield return frame with { Image = src.ApplyMaskToImage(mask) };
        }
    }
    
}
