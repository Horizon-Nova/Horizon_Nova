using System;
using System.Collections.Generic;
using HNB.IntelligentSystems.ObjectDetection.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

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

        try
        {
            var base64 = base64Data.Contains(",")
                ? base64Data.Split(',')[1]
                : base64Data;
            var imageBytes = Convert.FromBase64String(base64);
            return (true, imageBytes, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"base64 解碼失敗：{ex.Message}");
        }
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
    public static Image<Rgb24> DecodeImage(byte[] imageBytes)
    {
        return Image.Load<Rgb24>(imageBytes);
    }

    /// <summary>
    /// 在圖片上繪製檢測結果
    /// </summary>
    public static void DrawDetections(Image<Rgb24> image, List<DetectionResult> detections)
    {
        image.Mutate(ctx =>
        {
            foreach (var detection in detections)
            {
                // 繪製矩形框（紅色，2px）
                var rect = new RectangleF(
                    detection.Box.X,
                    detection.Box.Y,
                    detection.Box.Width,
                    detection.Box.Height
                );
                ctx.Draw(Color.Red, 2, rect);

                // 繪製標籤文字（簡化版本，不使用字體檔案）
                // ImageSharp 的文字繪製需要字體檔案，暫時簡化
            }
        });
    }

    /// <summary>
    /// 裁剪圖片區域
    /// </summary>
    public static Image<Rgb24> CropBox(Image<Rgb24> image, Models.Rectangle box)
    {
        var roi = new SixLabors.ImageSharp.Rectangle(
            Math.Max(0, box.X),
            Math.Max(0, box.Y),
            Math.Min(box.Width, image.Width - box.X),
            Math.Min(box.Height, image.Height - box.Y)
        );

        var cropped = image.Clone(ctx => ctx.Crop(roi));
        return cropped;
    }

    /// <summary>
    /// 將圖片編碼為位元組陣列
    /// </summary>
    public static byte[] EncodeImage(Image<Rgb24> image, string extension = ".jpg")
    {
        using var ms = new System.IO.MemoryStream();
        if (extension.ToLower() == ".png")
            image.SaveAsPng(ms);
        else
            image.SaveAsJpeg(ms);
        
        return ms.ToArray();
    }

    /// <summary>
    /// 保存圖片到文件（從 Image 物件）
    /// </summary>
    public static void SaveImage(Image<Rgb24> image, string filePath)
    {
        var extension = System.IO.Path.GetExtension(filePath).ToLower();
        if (extension == ".png")
            image.SaveAsPng(filePath);
        else
            image.SaveAsJpeg(filePath);
    }

    /// <summary>
    /// 從位元組陣列保存圖片到文件
    /// </summary>
    public static void SaveImageFromBytes(byte[] imageBytes, string filePath)
    {
        System.IO.File.WriteAllBytes(filePath, imageBytes);
    }

    /// <summary>
    /// 模型預處理：將圖片轉換為模型輸入格式
    /// </summary>
    public static float[] PreprocessForModel(Image<Rgb24> srcImg)
    {
        // 調整大小
        var resized = srcImg.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(TargetSize[0], TargetSize[1]),
            Mode = ResizeMode.Max
        }));

        // 最終調整到目標尺寸
        if (resized.Width != TargetSize[0] || resized.Height != TargetSize[1])
        {
            resized.Mutate(ctx => ctx.Resize(TargetSize[0], TargetSize[1]));
        }

        int area = TargetSize[0] * TargetSize[1];
        float[] output = new float[3 * area];

        // 轉換為模型輸入格式 (CHW format, normalized)
        resized.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++)
                {
                    var pixel = pixelRow[x];
                    int idx = y * accessor.Width + x;

                    // Normalize and apply mean/std
                    float r = (pixel.R / 255.0f - Mean[0]) / Std[0];
                    float g = (pixel.G / 255.0f - Mean[1]) / Std[1];
                    float b = (pixel.B / 255.0f - Mean[2]) / Std[2];

                    // CHW format (channels first)
                    output[0 * area + idx] = r;
                    output[1 * area + idx] = g;
                    output[2 * area + idx] = b;
                }
            }
        });

        resized.Dispose();
        return output;
    }

    /// <summary>
    /// 取得模型目標尺寸
    /// </summary>
    public static int[] GetModelTargetSize() => (int[])TargetSize.Clone();
}



