using Models.HnbHnbBackoffice;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Backoffice.Repositories;

/// <summary>
/// 側欄導航資料存取層，負責處理導航項目的資料存取功能
/// </summary>
public class SidebarNavigationRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源
    /// <summary>
    /// 有效的導航項目查詢來源
    /// </summary>
    private IQueryable<vw_sidebar_navigation> ValidNavigations => db.vw_sidebar_navigations.OrderBy(n => n.sort_order);
    
    #endregion

    #region 專用查詢方法

    /// <summary>
    /// 查詢導航項目列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="parentCode">父項目篩選（null = 不過濾，空字串 = 只要根項目）</param>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<vw_sidebar_navigation> QueryNavigationList(string? searchTerm = null, string? parentCode = null, bool? isActive = null)
        => ValidNavigations
            .Where(na =>
                (string.IsNullOrEmpty(searchTerm) ||
                    (na.title != null && na.title.Contains(searchTerm)) ||
                    (na.code != null && na.code.Contains(searchTerm))) &&
                (parentCode == null ||
                    (parentCode == "" && (na.parent_code == null || na.parent_code == "")) ||
                    (parentCode != "" && na.parent_code == parentCode)) &&
                (!isActive.HasValue || na.is_active == isActive.Value)
            )
            .ToList();

    /// <summary>
    /// 查詢單一導航項目
    /// </summary>
    /// <param name="id">導航項目ID</param>
    public vw_sidebar_navigation? QueryNavigation(int? id)
        => ValidNavigations.FirstOrDefault(n => n.id == id);

    #endregion

    #region 基本 CRUD 操作
    
    /// <summary>
    /// 插入導航項目（新增或變更）
    /// </summary>
    public sidebar_navigation InsertNavigation(sidebar_navigation form)
    {
        var existingEntity = QueryNavigation(form.id) != null 
            ? db.sidebar_navigations.FirstOrDefault(n => n.id == form.id)
            : null;
        
        if (existingEntity == null)
        {
            form.created_at = DateTime.UtcNow;
            form.updated_at = DateTime.UtcNow;
            db.sidebar_navigations.Add(form);
            db.SaveChanges();
            return form;
        }
        
        existingEntity.title = form.title;
        existingEntity.code = form.code;
        existingEntity.url = form.url;
        existingEntity.icon = form.icon;
        existingEntity.sort_order = form.sort_order;
        existingEntity.parent_code = form.parent_code;
        existingEntity.is_active = form.is_active;
        existingEntity.updated_at = DateTime.UtcNow;
      
        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 刪除導航項目
    /// </summary>
    public bool DeleteNavigation(int id)
    {
        if (QueryNavigation(id) == null) return false;
        
        var entity = db.sidebar_navigations.FirstOrDefault(n => n.id == id);
        if (entity != null)
        {
            db.sidebar_navigations.Remove(entity);
            db.SaveChanges();
            return true;
        }
        return false;
    }

    #endregion

}