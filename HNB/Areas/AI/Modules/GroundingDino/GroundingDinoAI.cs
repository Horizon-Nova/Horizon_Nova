using System.Drawing;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using HNB.Areas.AI.DataModels;
using HNB.Areas.AI.Utilities;

namespace HNB.Areas.AI.Modules.GroundingDino;

public static class GroundingDinoAI
{
    private static readonly InferenceSession _session;

    static GroundingDinoAI()
    {
        var modelPath = Path.Combine("Areas", "AI", "Modules", "GroundingDino", "model.onnx");
        _session = new InferenceSession(modelPath);
    }

    /// <summary>
    /// 執行 Grounding DINO 推論
    /// </summary>
    /// <param name="image">輸入圖片</param>
    /// <returns>推論結果的列表</returns>
    public static List<GroundingDinoResult> Run(Image image)
    {
        var tensor = ImageUtils.Preprocess(image);

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_image", tensor)
            // 若還有 text_embedding 或其他欄位，可以在這裡補上
        };

        using var results = _session.Run(inputs);
        return Postprocess(results);
    }

    /// <summary>
    /// 處理推論結果為 GroundingDinoResult 清單
    /// </summary>
    /// <param name="results">ONNX 執行結果</param>
    /// <returns>結果清單</returns>
    private static List<GroundingDinoResult> Postprocess(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
    {
        // TODO: 這邊需要你根據模型輸出的實際內容（tensor name, shape）去取值與轉換。
        // 以下是示意範本（請依照你的 ONNX 模型結構調整）

        var outputList = new List<GroundingDinoResult>();

        // 範例（假設模型輸出有 "boxes", "labels", "scores"）
        var boxes = results.FirstOrDefault(x => x.Name == "boxes")?.AsEnumerable<float>().ToArray();
        var labels = results.FirstOrDefault(x => x.Name == "labels")?.AsEnumerable<long>().ToArray();
        var scores = results.FirstOrDefault(x => x.Name == "scores")?.AsEnumerable<float>().ToArray();

        if (boxes == null || labels == null || scores == null)
            return outputList;

        int boxCount = labels.Length;
        for (int i = 0; i < boxCount; i++)
        {
            var result = new GroundingDinoResult
            {
                Id = i + 1,
                Phrase = $"Label_{labels[i]}", // 若有對應 phrase 清單可改
                Logit = scores[i],
                BBoxNorm = new List<float> {
                    boxes[i * 4 + 0],
                    boxes[i * 4 + 1],
                    boxes[i * 4 + 2],
                    boxes[i * 4 + 3]
                },
                ImageName = "", // 可視需要填入
                ImageSize = new ImageSize { Width = 0, Height = 0 },
                InputImageSize = new ImageSize { Width = 0, Height = 0 },
                ScaleRatio = new ScaleRatio { X = 1f, Y = 1f }
            };

            outputList.Add(result);
        }

        return outputList;
    }
}
