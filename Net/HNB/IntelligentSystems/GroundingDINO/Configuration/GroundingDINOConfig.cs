namespace HNB.IntelligentSystems.GroundingDINO.Configuration;

public class GroundingDINOConfig
{
    public string ModelPath { get; set; } = "";
    public string VocabPath { get; set; } = "";
    public string TextPrompt { get; set; } = "";
    public float BoxThreshold { get; set; } = 0.25f;
    public float TextThreshold { get; set; } = 0.2f;
    public bool IncludeLogits { get; set; } = true;
}

