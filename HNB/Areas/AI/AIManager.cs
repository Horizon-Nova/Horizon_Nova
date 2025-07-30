using System.Drawing;
using HNB.Areas.AI.DataModels;
using HNB.Areas.AI.Utilities;

namespace HNB.Areas.AI;

public static class AIManager
{
    /// <summary>
    /// 執行 GroundingDINO 模型推論
    /// </summary>
    public static List<GroundingDinoResult> GroundingDino(Image image)
    {
        return Modules.GroundingDino.GroundingDinoAI.Run(image);
    }
}
