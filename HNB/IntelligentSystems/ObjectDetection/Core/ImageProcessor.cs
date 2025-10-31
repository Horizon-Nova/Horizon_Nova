using System;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Core;

public static class ImageProcessor
{
    private static readonly float[] Mean = { 0.485f, 0.456f, 0.406f };
    private static readonly float[] Std = { 0.229f, 0.224f, 0.225f };
    private static readonly int[] TargetSize = { 1200, 800 };

    public static float[] Preprocess(Mat srcImg)
    {
        Mat enhanced = EnhanceImage(srcImg);
        Mat resized = ResizeImage(enhanced, TargetSize);
        enhanced.Dispose();

        Mat rgbImg = new Mat();
        Cv2.CvtColor(resized, rgbImg, ColorConversionCodes.BGR2RGB);
        resized.Dispose();

        Mat finalImg = new Mat();
        Cv2.Resize(rgbImg, finalImg, new Size(TargetSize[0], TargetSize[1]));
        rgbImg.Dispose();

        Mat normalized = new Mat();
        finalImg.ConvertTo(normalized, MatType.CV_32FC3, 1.0 / 255.0);
        finalImg.Dispose();

        Mat[] channels = Cv2.Split(normalized);
        int area = TargetSize[0] * TargetSize[1];
        float[] output = new float[3 * area];

        for (int c = 0; c < 3; c++)
        {
            channels[c].ConvertTo(channels[c], MatType.CV_32F, 1.0 / Std[c], -Mean[c] / Std[c]);

            unsafe
            {
                float* ptr = (float*)channels[c].DataPointer;
                for (int i = 0; i < area; i++)
                {
                    output[c * area + i] = ptr[i];
                }
            }
            channels[c].Dispose();
        }

        normalized.Dispose();
        return output;
    }

    private static Mat EnhanceImage(Mat srcImg)
    {
        Mat enhanced = new Mat();
        Mat lab = new Mat();
        Cv2.CvtColor(srcImg, lab, ColorConversionCodes.BGR2Lab);
        Mat[] labChannels = Cv2.Split(lab);
        Mat lChannel = labChannels[0];
        Mat enhancedL = new Mat();
        Cv2.CreateCLAHE(clipLimit: 2.0, tileGridSize: new Size(8, 8)).Apply(lChannel, enhancedL);
        Mat[] enhancedLabChannels = { enhancedL, labChannels[1], labChannels[2] };
        Cv2.Merge(enhancedLabChannels, lab);
        Cv2.CvtColor(lab, enhanced, ColorConversionCodes.Lab2BGR);
        lChannel.Dispose();
        enhancedL.Dispose();
        lab.Dispose();
        foreach (var channel in labChannels)
            channel.Dispose();
        
        Mat sharpened = new Mat();
        using (var kernel = new Mat(3, 3, MatType.CV_32FC1))
        {
            unsafe
            {
                float* ptr = (float*)kernel.DataPointer;
                ptr[0] = 0; ptr[1] = -1; ptr[2] = 0;
                ptr[3] = -1; ptr[4] = 5; ptr[5] = -1;
                ptr[6] = 0; ptr[7] = -1; ptr[8] = 0;
            }
            Cv2.Filter2D(enhanced, sharpened, MatType.CV_8UC3, kernel);
        }
        Mat result = new Mat();
        Cv2.AddWeighted(sharpened, 0.7, enhanced, 0.3, 0, result);
        enhanced.Dispose();
        sharpened.Dispose();
        return result;
    }

    private static Mat ResizeImage(Mat srcImg, int[] targetSize)
    {
        int srcW = srcImg.Width;
        int srcH = srcImg.Height;
        int targetW = targetSize[0];
        int targetH = targetSize[1];
        double scaleW = (double)targetW / srcW;
        double scaleH = (double)targetH / srcH;
        double scale = Math.Min(scaleW, scaleH);
        int newW = (int)(srcW * scale);
        int newH = (int)(srcH * scale);
        Mat resized = new Mat();
        Cv2.Resize(srcImg, resized, new Size(newW, newH), 0, 0, InterpolationFlags.Linear);
        return resized;
    }

    public static int[] GetTargetSize() => (int[])TargetSize.Clone();
}

