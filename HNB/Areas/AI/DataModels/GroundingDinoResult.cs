namespace HNB.Areas.AI.DataModels;

/// <summary>
/// Grounding DINO 推論的結果模型（對應 Python 回傳的 dict）
/// </summary>
public class GroundingDinoResult
{
    /// <summary>
    /// 物件 ID（通常是序列編號，從 1 開始）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 匹配到的文字片語，例如 "shirt", "car", "dial"
    /// </summary>
    public string Phrase { get; set; } = "";

    /// <summary>
    /// 模型給出的信心分數，通常 0~1 之間的小數
    /// </summary>
    public float Logit { get; set; }

    /// <summary>
    /// 模型輸出的 bounding box，使用「相對座標」(0~1)
    /// 格式為 [x_min, y_min, x_max, y_max]
    /// </summary>
    public List<float> BBoxNorm { get; set; } = new();

    /// <summary>
    /// 圖片名稱（含副檔名）
    /// </summary>
    public string ImageName { get; set; } = "";

    /// <summary>
    /// 原始輸入圖片的寬與高
    /// </summary>
    public ImageSize ImageSize { get; set; } = new();

    /// <summary>
    /// 模型輸入用的圖片尺寸（通常會被 resize）
    /// </summary>
    public ImageSize InputImageSize { get; set; } = new();

    /// <summary>
    /// 圖片縮放倍率（原圖 / 模型輸入）
    /// </summary>
    public ScaleRatio ScaleRatio { get; set; } = new();
}

/// <summary>
/// 表示圖片尺寸（寬與高）
/// </summary>
public class ImageSize
{
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// 表示圖片縮放比例
/// </summary>
public class ScaleRatio
{
    public float X { get; set; }
    public float Y { get; set; }
}
