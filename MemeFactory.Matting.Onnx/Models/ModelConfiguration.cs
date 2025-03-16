namespace MemeFactory.Matting.Onnx.Models;

public abstract class ModelConfiguration
{
    public string ModelPath { get; }
    public abstract int InputWidth { get; }
    public abstract int InputHeight { get; }

    protected abstract float RedNormalizationMean { get; }
    protected abstract float GreenNormalizationMean { get; }
    protected abstract float BlueNormalizationMean { get; }
    protected abstract float RedNormalizationStd { get; }
    protected abstract float GreenNormalizationStd { get; }
    protected abstract float BlueNormalizationStd { get; }

    public ModelConfiguration(string modelPath)
    {
        ModelPath = modelPath;
    }

    public float NormalizeRed(float value)
    {
        return Normalize(value, RedNormalizationMean, RedNormalizationStd);
    }

    public float NormalizeGreen(float value)
    {
        return Normalize(value, GreenNormalizationMean, GreenNormalizationStd);
    }

    public float NormalizeBlue(float value)
    {
        return Normalize(value, BlueNormalizationMean, BlueNormalizationStd);
    }

    private static float Normalize(float value, float mean, float std)
    {
        return ((value / 255f) - mean) / std;
    }
}