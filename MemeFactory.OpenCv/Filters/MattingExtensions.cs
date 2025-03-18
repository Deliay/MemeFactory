using MemeFactory.Core.Processing;
using OpenCvSharp;
using Range = System.Range;

namespace MemeFactory.OpenCv.Filters;

public static partial class MemeCv
{
    public static Func<Mat, ValueTask<Mat>> FloodMatting(Point point, int colorThreshold = 10)
        => mat => ValueTask.FromResult(FloodMatting(mat, point, colorThreshold)); 

    private static Mat FloodMatting(Mat mat, Point point, float colorThreshold = 30f)
    {
        using var mask = new Mat();
        var threshold = new Scalar(colorThreshold, colorThreshold, colorThreshold);
        mat.FloodFill(mask, point, Scalar.Black, out _, threshold, threshold, FloodFillFlags.MaskOnly);
        Cv2.BitwiseNot(mask, mask);
            
        using var sliceMask = mask[new Range(1, Index.FromEnd(1)), new Range(1, Index.FromEnd(1))]; 
            
        using var thresholdMask = sliceMask.Threshold(127, 255, ThresholdTypes.Binary);
            
        using var result = new Mat();
        Cv2.BitwiseAnd(mat, mat, result, thresholdMask);
            
        var output = new Mat(mat.Rows, mat.Cols, MatType.CV_8UC4);
        Cv2.Merge([result, thresholdMask], output);

        return output;
    }
}