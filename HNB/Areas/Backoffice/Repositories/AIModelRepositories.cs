using Models.HnbHnbBackoffice;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// AI 模型配置資料存取層
/// </summary>
public class AIModelRepositories(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源

    /// <summary>
    /// 有效的 AI 配置查詢來源 (視圖)
    /// </summary>
    private IQueryable<vw_ai_config> ValidAIConfigs => db.vw_ai_configs.OrderBy(c => c.priority);

    /// <summary>
    /// 有效的 AI 日誌查詢來源 (視圖)
    /// </summary>
    private IQueryable<vw_ai_log> ValidAILogs => db.vw_ai_logs
        .OrderByDescending(l => l.created_at);

    /// <summary>
    /// 有效的 AI 配置查詢來源 (實體表)
    /// </summary>
    private IQueryable<ai_config> ValidAIConfigEntities => db.ai_configs
        .Where(c => c.deleted_at == null)
        .OrderBy(c => c.priority);

    /// <summary>
    /// 有效的 AI 日誌查詢來源 (實體表)
    /// </summary>
    private IQueryable<ai_log> ValidAILogEntities => db.ai_logs
        .OrderByDescending(l => l.created_at);

    #endregion

    #region 專用查詢方法

    // 表1: vw_ai_config (視圖)
    /// <summary>
    /// 查詢 AI 配置列表 (視圖)
    /// </summary>
    public List<vw_ai_config> QueryAIConfigList(
        string? searchTerm = null,
        string? provider = null,
        string? scope = null,
        bool? isEnabled = null)
        => ValidAIConfigs
            .Where(c => string.IsNullOrEmpty(searchTerm) ||
                       (c.service_name != null && c.service_name.Contains(searchTerm)) ||
                       (c.provider != null && c.provider.Contains(searchTerm)) ||
                       (c.model_key != null && c.model_key.Contains(searchTerm)))
            .Where(c => string.IsNullOrEmpty(provider) || c.provider == provider)
            .Where(c => string.IsNullOrEmpty(scope) || c.scope == scope)
            .Where(c => !isEnabled.HasValue || c.is_enabled == isEnabled.Value)
            .ToList();

    /// <summary>
    /// 查詢單一 AI 配置 (視圖)
    /// </summary>
    public vw_ai_config? QueryAIConfig(long? id = null, string? modelKey = null)
    {
        if (id.HasValue)
            return ValidAIConfigs.FirstOrDefault(c => c.id == id.Value);

        if (!string.IsNullOrEmpty(modelKey))
            return ValidAIConfigs.FirstOrDefault(c => c.model_key == modelKey);

        return null;
    }

    // 表2: vw_ai_log (視圖)
    /// <summary>
    /// 查詢 AI 日誌列表 (視圖)
    /// </summary>
    public List<vw_ai_log> QueryAILogList(
        long? configId = null,
        int? userId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null)
    {
        var query = ValidAILogs
            .Where(l => !configId.HasValue || l.ai_config_id == configId.Value)
            .Where(l => !userId.HasValue || l.user_id == userId.Value)
            .Where(l => string.IsNullOrEmpty(status) || l.status == status)
            .Where(l => !startDate.HasValue || l.created_at >= startDate.Value)
            .Where(l => !endDate.HasValue || l.created_at <= endDate.Value);

        return limit.HasValue ? query.Take(limit.Value).ToList() : query.ToList();
    }

    /// <summary>
    /// 查詢單一 AI 日誌 (視圖)
    /// </summary>
    public vw_ai_log? QueryAILog(long? id = null)
        => id.HasValue ? ValidAILogs.FirstOrDefault(l => l.id == id.Value) : null;

    // 表3: ai_config (實體表)
    /// <summary>
    /// 查詢 AI 配置列表 (實體表)
    /// </summary>
    public List<ai_config> QueryAIConfigEntityList(
        string? searchTerm = null,
        string? provider = null,
        string? scope = null,
        bool? isEnabled = null)
        => ValidAIConfigEntities
            .Where(c => string.IsNullOrEmpty(searchTerm) ||
                       c.service_name.Contains(searchTerm) ||
                       c.provider.Contains(searchTerm) ||
                       c.model_key.Contains(searchTerm))
            .Where(c => string.IsNullOrEmpty(provider) || c.provider == provider)
            .Where(c => string.IsNullOrEmpty(scope) || c.scope == scope)
            .Where(c => !isEnabled.HasValue || c.is_enabled == isEnabled.Value)
            .ToList();

    /// <summary>
    /// 查詢單一 AI 配置 (實體表)
    /// </summary>
    public ai_config? QueryAIConfigEntity(long? id = null, string? modelKey = null)
    {
        if (id.HasValue)
            return ValidAIConfigEntities.FirstOrDefault(c => c.id == id.Value);

        if (!string.IsNullOrEmpty(modelKey))
            return ValidAIConfigEntities.FirstOrDefault(c => c.model_key == modelKey);

        return null;
    }

    // 表4: ai_log (實體表)
    /// <summary>
    /// 查詢 AI 日誌列表 (實體表)
    /// </summary>
    public List<ai_log> QueryAILogEntityList(
        long? configId = null,
        int? userId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null)
    {
        var query = ValidAILogEntities
            .Where(l => !configId.HasValue || l.ai_config_id == configId.Value)
            .Where(l => !userId.HasValue || l.user_id == userId.Value)
            .Where(l => string.IsNullOrEmpty(status) || l.status == status)
            .Where(l => !startDate.HasValue || l.created_at >= startDate.Value)
            .Where(l => !endDate.HasValue || l.created_at <= endDate.Value);

        return limit.HasValue ? query.Take(limit.Value).ToList() : query.ToList();
    }

    /// <summary>
    /// 查詢單一 AI 日誌 (實體表)
    /// </summary>
    public ai_log? QueryAILogEntity(long? id = null)
        => id.HasValue ? ValidAILogEntities.FirstOrDefault(l => l.id == id.Value) : null;

    #endregion

    #region 基本 CRUD 操作

    /// <summary>
    /// 插入 AI 配置（新增或更新）
    /// </summary>
    public ai_config InsertAIConfig(ai_config data)
    {
        var existingEntity = db.ai_configs.Find(data.id);

        if (existingEntity == null)
        {
            // 新增邏輯
            data.created_at = DateTime.Now;
            db.ai_configs.Add(data);
            db.SaveChanges();
            return data;
        }

        // 更新邏輯
        existingEntity.service_name = data.service_name;
        existingEntity.provider = data.provider;
        existingEntity.scope = data.scope;
        existingEntity.model_key = data.model_key;
        existingEntity.version = data.version;
        existingEntity.description = data.description;
        existingEntity.changelog = data.changelog;
        existingEntity.system_prompt = data.system_prompt;
        existingEntity.field_names = data.field_names;
        existingEntity.field_labels = data.field_labels;
        existingEntity.field_types = data.field_types;
        existingEntity.field_values = data.field_values;
        existingEntity.field_required = data.field_required;
        existingEntity.field_sensitive = data.field_sensitive;
        existingEntity.field_enabled = data.field_enabled;
        existingEntity.quality_score = data.quality_score;
        existingEntity.monthly_budget = data.monthly_budget;
        existingEntity.yearly_budget = data.yearly_budget;
        existingEntity.budget_alert_threshold = data.budget_alert_threshold;
        existingEntity.is_enabled = data.is_enabled;
        existingEntity.priority = data.priority;
        existingEntity.daily_limit = data.daily_limit;
        existingEntity.updated_at = DateTime.Now;
        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 插入 AI 日誌（新增或更新）
    /// </summary>
    public ai_log InsertAILog(ai_log data)
    {
        var existingEntity = db.ai_logs.Find(data.id);

        if (existingEntity == null)
        {
            // 新增邏輯
            data.created_at = DateTime.Now;
            db.ai_logs.Add(data);
            db.SaveChanges();
            return data;
        }

        // 更新邏輯
        existingEntity.ai_config_id = data.ai_config_id;
        existingEntity.user_id = data.user_id;
        existingEntity.request_type = data.request_type;
        existingEntity.request_data = data.request_data;
        existingEntity.response_data = data.response_data;
        existingEntity.status = data.status;
        existingEntity.response_time_ms = data.response_time_ms;
        existingEntity.tokens_used = data.tokens_used;
        existingEntity.cost = data.cost;
        existingEntity.error_message = data.error_message;
        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 刪除 AI 配置（軟刪除）
    /// </summary>
    public bool DeleteAIConfig(long id)
    {
        var entity = db.ai_configs.Find(id);
        if (entity != null)
        {
            entity.deleted_at = DateTime.Now;
            db.SaveChanges();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 刪除 AI 日誌（物理刪除）
    /// </summary>
    public bool DeleteAILog(long id)
    {
        var entity = db.ai_logs.Find(id);
        if (entity != null)
        {
            db.ai_logs.Remove(entity);
            db.SaveChanges();
            return true;
        }
        return false;
    }

    #endregion
}
