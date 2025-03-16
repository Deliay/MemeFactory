namespace MemeFactory.Matting.Onnx.Models;

public class RmbgConfiguration(string modelPath) : ModelConfiguration(modelPath)
{
    public override int InputWidth => 1024;
    public override int InputHeight => 1024;
    protected override float RedNormalizationMean => 0.485f;
    protected override float GreenNormalizationMean => 0.456f;
    protected override float BlueNormalizationMean => 0.406f;
    protected override float RedNormalizationStd => 0.229f;
    protected override float GreenNormalizationStd => 0.224f;
    protected override float BlueNormalizationStd => 0.225f;
}