using System.Drawing;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace HNB.Areas.AI.Utilities;

public static class ImageUtils
{
    public static DenseTensor<float> Preprocess(Image image)
    {
        int targetWidth = 224;
        int targetHeight = 224;

        using var bmp = new Bitmap(image, new Size(targetWidth, targetHeight));
        var tensor = new DenseTensor<float>(new[] { 1, 3, targetHeight, targetWidth });

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                Color color = bmp.GetPixel(x, y);
                tensor[0, 0, y, x] = color.R / 255f;
                tensor[0, 1, y, x] = color.G / 255f;
                tensor[0, 2, y, x] = color.B / 255f;
            }
        }

        return tensor;
    }
}
