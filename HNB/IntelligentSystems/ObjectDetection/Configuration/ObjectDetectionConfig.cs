namespace HNB.IntelligentSystems.ObjectDetection.Configuration;

public class ObjectDetectionConfig
{
    public string ModelPath { get; set; } = "";
    public string VocabPath { get; set; } = "";
    public string TextPrompt { get; set; } = "jacket . clothes . pants . shoes .";
    public float BoxThreshold { get; set; } = 0.25f;
    public float TextThreshold { get; set; } = 0.2f;
    public bool IncludeLogits { get; set; } = true;
}

