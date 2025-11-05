using System;
using System.Collections.Generic;
using HNB.IntelligentSystems.ObjectDetection.Models;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Utils;

public static class ImageUtils
{
    // 模型預處理參數
    private static readonly float[] Mean = { 0.485f, 0.456f, 0.406f };
    private static readonly float[] Std = { 0.229f, 0.224f, 0.225f };
    private static readonly int[] TargetSize = { 1200, 800 };

    /// <summary>
    /// 解析 base64 圖片資料為位元組陣列
    /// </summary>
    public static (bool success, byte[]? imageBytes, string? error) ParseBase64Image(string base64Data)
    {
        if (string.IsNullOrEmpty(base64Data))
            return (false, null, "base64 圖片資料不能為空");

        var base64 = base64Data.Contains(",")
            ? base64Data.Split(',')[1]
            : base64Data;
        var imageBytes = Convert.FromBase64String(base64);
        return (true, imageBytes, null);
    }

    /// <summary>
    /// 批次解析 base64 圖片資料為位元組陣列
    /// </summary>
    public static (List<byte[]> validImages, List<string> errors) ParseBase64Images(List<string> base64Images)
    {
        var validImages = new List<byte[]>();
        var errors = new List<string>();

        foreach (var imageBase64 in base64Images)
        {
            var (parseSuccess, imageBytes, parseError) = ParseBase64Image(imageBase64);
            if (!parseSuccess || imageBytes == null)
            {
                errors.Add(parseError ?? "無效的 base64 圖片資料");
                continue;
            }
            validImages.Add(imageBytes);
        }

        return (validImages, errors);
    }

    /// <summary>
    /// 從 IFormFile 讀取圖片位元組陣列
    /// </summary>
    public static async Task<(bool success, byte[]? imageBytes, string? error)> ReadFormFileImage(Microsoft.AspNetCore.Http.IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (false, null, $"圖片 {file?.FileName ?? "未知"} 為空");

        using var ms = new System.IO.MemoryStream();
        await file.CopyToAsync(ms);
        return (true, ms.ToArray(), null);
    }

    /// <summary>
    /// 批次從 IFormFile 讀取圖片位元組陣列
    /// </summary>
    public static async Task<(List<byte[]> validImages, List<string> errors)> ReadFormFileImages(List<Microsoft.AspNetCore.Http.IFormFile> files)
    {
        var validImages = new List<byte[]>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            var (success, imageBytes, error) = await ReadFormFileImage(file);
            if (!success || imageBytes == null)
            {
                errors.Add(error ?? $"圖片 {file?.FileName ?? "未知"} 讀取失敗");
                continue;
            }
            validImages.Add(imageBytes);
        }

        return (validImages, errors);
    }

    /// <summary>
    /// 從位元組陣列解碼圖片
    /// </summary>
    public static Mat? DecodeImage(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return null;

        var image = Cv2.ImDecode(imageBytes, ImreadModes.Color);
        return image.Empty() ? null : image;
    }

    /// <summary>
    /// 在圖片上繪製檢測結果
    /// </summary>
    public static void DrawDetections(Mat image, List<DetectionResult> detections)
    {
        foreach (var detection in detections)
        {
            Cv2.Rectangle(image, detection.Box, new Scalar(0, 0, 255), 2);
            Point labelPos = new Point(detection.Box.X, detection.Box.Y - 5);
            Cv2.PutText(image, detection.Label, labelPos,
                HersheyFonts.HersheySimplex, 0.7, new Scalar(0, 255, 0), 2);
        }
    }

    /// <summary>
    /// 裁剪圖片區域
    /// </summary>
    public static Mat CropBox(Mat image, Rect box)
    {
        var roi = new Rect(
            System.Math.Max(0, box.X),
            System.Math.Max(0, box.Y),
            System.Math.Min(box.Width, image.Width - box.X),
            System.Math.Min(box.Height, image.Height - box.Y)
        );
        return new Mat(image, roi);
    }

    /// <summary>
    /// 將圖片編碼為位元組陣列
    /// </summary>
    public static byte[] EncodeImage(Mat image, string extension = ".jpg")
    {
        return image.ImEncode(extension);
    }

    /// <summary>
    /// 保存圖片到文件
    /// </summary>
    public static bool SaveImage(Mat image, string filePath)
    {
        if (image == null || image.Empty())
            return false;

        try
        {
            Cv2.ImWrite(filePath, image);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 從位元組陣列保存圖片到文件
    /// </summary>
    public static bool SaveImageFromBytes(byte[] imageBytes, string filePath)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return false;

        try
        {
            System.IO.File.WriteAllBytes(filePath, imageBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 模型預處理：將圖片轉換為模型輸入格式
    /// </summary>
    public static float[] PreprocessForModel(Mat srcImg)
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

    /// <summary>
    /// 增強圖片（對比度和銳化）
    /// </summary>
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

    /// <summary>
    /// 調整圖片大小（保持長寬比）
    /// </summary>
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

    /// <summary>
    /// 取得模型目標尺寸
    /// </summary>
    public static int[] GetModelTargetSize() => (int[])TargetSize.Clone();
}

