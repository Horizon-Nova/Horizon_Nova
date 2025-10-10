using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Services;

/// <summary>
/// AI 模型配置服務層
/// </summary>
public class AIModelServices(AIModelRepositories rep)
{
    #region 統一的查詢方法

    // 表1: vw_ai_config (視圖)
    /// <summary>
    /// 載入 AI 配置列表 (視圖)
    /// </summary>
    public List<vw_ai_config> LoadAIConfigList(
        string? searchTerm = null,
        string? provider = null,
        string? scope = null,
        bool? isEnabled = null)
        => rep.QueryAIConfigList(searchTerm, provider, scope, isEnabled);

    /// <summary>
    /// 載入單一 AI 配置 (視圖)
    /// </summary>
    public vw_ai_config? LoadAIConfig(long? id = null, string? modelKey = null)
        => rep.QueryAIConfig(id, modelKey);

    // 表2: vw_ai_log (視圖)
    /// <summary>
    /// 載入 AI 日誌列表 (視圖)
    /// </summary>
    public List<vw_ai_log> LoadAILogList(
        long? configId = null,
        int? userId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null)
        => rep.QueryAILogList(configId, userId, status, startDate, endDate, limit);

    /// <summary>
    /// 載入單一 AI 日誌 (視圖)
    /// </summary>
    public vw_ai_log? LoadAILog(long? id = null)
        => rep.QueryAILog(id);

    // 表3: ai_config (實體表)
    /// <summary>
    /// 載入 AI 配置列表 (實體表)
    /// </summary>
    public List<ai_config> LoadAIConfigEntityList(
        string? searchTerm = null,
        string? provider = null,
        string? scope = null,
        bool? isEnabled = null)
        => rep.QueryAIConfigEntityList(searchTerm, provider, scope, isEnabled);

    /// <summary>
    /// 載入單一 AI 配置 (實體表)
    /// </summary>
    public ai_config? LoadAIConfigEntity(long? id = null, string? modelKey = null)
        => rep.QueryAIConfigEntity(id, modelKey);

    // 表4: ai_log (實體表)
    /// <summary>
    /// 載入 AI 日誌列表 (實體表)
    /// </summary>
    public List<ai_log> LoadAILogEntityList(
        long? configId = null,
        int? userId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null)
        => rep.QueryAILogEntityList(configId, userId, status, startDate, endDate, limit);

    /// <summary>
    /// 載入單一 AI 日誌 (實體表)
    /// </summary>
    public ai_log? LoadAILogEntity(long? id = null)
        => rep.QueryAILogEntity(id);

    #endregion

    #region ViewBag 設定方法

    /// <summary>
    /// 設定 AI 配置管理頁面的 ViewBag
    /// </summary>
    public void ViewBagModel(dynamic viewBag, long? id = null)
    {
        viewBag.Id = id;
        viewBag.AIConfigs = LoadAIConfigList();
        viewBag.AIConfig = id.HasValue ? LoadAIConfig(id.Value) : null;
    }

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 創建或更新 AI 配置
    /// </summary>
    public ai_config CreateAIConfig(ai_config data)
        => rep.InsertAIConfig(data);

    /// <summary>
    /// 創建或更新 AI 日誌
    /// </summary>
    public ai_log CreateAILog(ai_log data)
        => rep.InsertAILog(data);

    /// <summary>
    /// 刪除 AI 配置
    /// </summary>
    public bool DeleteAIConfig(long id)
        => rep.DeleteAIConfig(id);

    /// <summary>
    /// 刪除 AI 日誌
    /// </summary>
    public bool DeleteAILog(long id)
        => rep.DeleteAILog(id);

    #endregion

}
