using System.Text;
using System.Text.Json;
using Microsoft.ML.OnnxRuntime; // ← 只需要這個
// no: using Microsoft.ML.OnnxRuntime.Metadata;

namespace HNB.Areas.AI.Modules.GroundingDino;

public static class GroundingDinoHelper
{
    public static GroundingDinoModelInfo Info => _info.Value;
    private static readonly Lazy<GroundingDinoModelInfo> _info = new(BuildInfo);

    public static string ToJson(bool indented = true) =>
        JsonSerializer.Serialize(Info, new JsonSerializerOptions { WriteIndented = indented });

    public static string ToMarkdown()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {Info.Name}");
        sb.AppendLine();
        sb.AppendLine($"**Description:** {Info.Description}");
        sb.AppendLine();
        sb.AppendLine("## Capabilities");
        foreach (var c in Info.Capabilities) sb.AppendLine($"- {c}");
        sb.AppendLine();
        sb.AppendLine("## Default Thresholds");
        sb.AppendLine($"- BoxThreshold: {Info.DefaultBoxThreshold}");
        sb.AppendLine($"- TextThreshold: {Info.DefaultTextThreshold}");
        sb.AppendLine();
        sb.AppendLine("## Prompt Example");
        sb.AppendLine("```");
        sb.AppendLine(Info.PromptExample);
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Inputs");
        foreach (var t in Info.Inputs)
            sb.AppendLine($"- `{t.Name}` : {t.ElementType} [{string.Join(" x ", t.Shape.Select(d => d.ToString()))}]");
        sb.AppendLine();
        sb.AppendLine("## Outputs");
        foreach (var t in Info.Outputs)
            sb.AppendLine($"- `{t.Name}` : {t.ElementType} [{string.Join(" x ", t.Shape.Select(d => d.ToString()))}]");
        return sb.ToString();
    }

    private static GroundingDinoModelInfo BuildInfo()
    {
        var modelPath = Path.Combine("Areas", "AI", "Modules", "GroundingDino", "model.onnx");
        using var session = new InferenceSession(modelPath);

        return new GroundingDinoModelInfo
        {
            Name = "GroundingDINO (ONNX)",
            Version = "tiny / community build",
            Description = "Open-set phrase grounding model. Image + text prompt → boxes with phrases.",
            Capabilities = new[] { "Open-vocabulary detection", "Phrase grounding", "Box prediction" },
            DefaultBoxThreshold = 0.4f,
            DefaultTextThreshold = 0.3f,
            PromptExample = "shirt . pants . shoes . jacket . dial .",
            Inputs = ToTensorMetaList(session.InputMetadata),
            Outputs = ToTensorMetaList(session.OutputMetadata),
            Notes = "Text must be embedded (CLIP). Image normalized to required size."
        };
    }

    private static List<TensorMeta> ToTensorMetaList(IReadOnlyDictionary<string, NodeMetadata> dict)
        => dict.Select(kv => new TensorMeta
        {
            Name = kv.Key,
            ElementType = kv.Value.ElementType.ToString(),
            // kv.Value.Dimensions 可能是 int[] 或 long[]，用 Convert.ToInt64 保險
            Shape = kv.Value.Dimensions.Select(d => Convert.ToInt64(d)).ToArray()
        }).ToList();
}

public class GroundingDinoModelInfo
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public string[] Capabilities { get; set; } = Array.Empty<string>();
    public float DefaultBoxThreshold { get; set; }
    public float DefaultTextThreshold { get; set; }
    public string PromptExample { get; set; } = "";
    public List<TensorMeta> Inputs { get; set; } = new();
    public List<TensorMeta> Outputs { get; set; } = new();
    public string Notes { get; set; } = "";
}

public class TensorMeta
{
    public string Name { get; set; } = "";
    public string ElementType { get; set; } = "";
    public long[] Shape { get; set; } = Array.Empty<long>();
}
