using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;

namespace HNB.Areas.AI.Utilities;

public static class OnnxModelInspector
{
    public static OnnxModelInfo GetModelInfo(string modelPath)
    {
        using var session = new InferenceSession(modelPath);
        var meta = session.ModelMetadata;

        var inputs = session.InputMetadata.Select(kv => new OnnxTensorInfo
        {
            Name = kv.Key,
            ElementType = kv.Value.ElementType.ToString(),
            Dimensions = kv.Value.Dimensions.Select(d => d.ToString()).ToList()
        }).ToList();

        var outputs = session.OutputMetadata.Select(kv => new OnnxTensorInfo
        {
            Name = kv.Key,
            ElementType = kv.Value.ElementType.ToString(),
            Dimensions = kv.Value.Dimensions.Select(d => d.ToString()).ToList()
        }).ToList();

        return new OnnxModelInfo
        {
            ModelPath = modelPath,
            GraphName = meta.GraphName,
            ProducerName = meta.ProducerName,
            Description = meta.Description,
            Domain = meta.Domain,
            Inputs = inputs,
            Outputs = outputs
        };
    }
}

public class OnnxModelInfo
{
    public string ModelPath { get; set; } = "";
    public string GraphName { get; set; } = "";
    public string ProducerName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Domain { get; set; } = "";
    public List<OnnxTensorInfo> Inputs { get; set; } = new();
    public List<OnnxTensorInfo> Outputs { get; set; } = new();
}

public class OnnxTensorInfo
{
    public string Name { get; set; } = "";
    public string ElementType { get; set; } = "";
    public List<string> Dimensions { get; set; } = new();
}
