using System.Collections.Generic;
using HNB.IntelligentSystems.ObjectDetection.Models;
using OpenCvSharp;

namespace HNB.IntelligentSystems.ObjectDetection.Utils;

public static class ImageUtils
{
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
}

