using System.Text.Json;
namespace HNB.Areas.AI.Utilities;

public static class TokenizerInspector
{
    public static TokenizerInfo GetTokenizerInfo(string tokenizerPath)
    {
        if (!File.Exists(tokenizerPath))
            throw new FileNotFoundException("找不到 tokenizer.json: " + tokenizerPath);

        var json = File.ReadAllText(tokenizerPath);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        var modelType = root.GetProperty("model").GetProperty("type").GetString() ?? "";

        var vocab = root.GetProperty("model").GetProperty("vocab");
        var vocabKeys = vocab.EnumerateObject().Select(x => x.Name).ToList();

        return new TokenizerInfo
        {
            ModelType = modelType,
            VocabSize = vocabKeys.Count,
            HasCls = vocabKeys.Contains("[CLS]"),
            HasSep = vocabKeys.Contains("[SEP]"),
            HasPad = vocabKeys.Contains("[PAD]"),
            HasMask = vocabKeys.Contains("[MASK]"),
            SampleTokens = vocabKeys.Take(10).ToList()
        };
    }
}

public class TokenizerInfo
{
    public string ModelType { get; set; } = "";
    public int VocabSize { get; set; }
    public bool HasCls { get; set; }
    public bool HasSep { get; set; }
    public bool HasPad { get; set; }
    public bool HasMask { get; set; }
    public List<string> SampleTokens { get; set; } = new();
}