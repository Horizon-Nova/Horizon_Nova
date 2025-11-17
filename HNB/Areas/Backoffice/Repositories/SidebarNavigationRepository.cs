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
    /// 導航項目查詢來源
    /// </summary>
    private IQueryable<vw_sidebar_navigation> ValidNavigations => db.vw_sidebar_navigations.OrderBy(n => n.sort_order);
    
    #endregion

    #region 專用查詢方法

    /// <summary>
    /// 查詢導航項目列表
    /// </summary>
    public List<vw_sidebar_navigation> QueryNavigationList(vw_sidebar_navigation? form)
    {
        if (form == null) return ValidNavigations.ToList();

        return ValidNavigations
            .Where(na =>
                (!form.id.HasValue || na.id == form.id) &&
                (string.IsNullOrEmpty(form.code) || na.code == form.code) &&
                (string.IsNullOrEmpty(form.title) || na.title == form.title) &&
                (string.IsNullOrEmpty(form.url) || na.url == form.url) &&
                (string.IsNullOrEmpty(form.icon) || na.icon == form.icon) &&
                (string.IsNullOrEmpty(form.parent_code) || na.parent_code == form.parent_code) &&
                (!form.sort_order.HasValue || na.sort_order == form.sort_order) &&
                (!form.is_active.HasValue || na.is_active == form.is_active) &&
                (!form.created_at.HasValue || na.created_at == form.created_at) &&
                (!form.updated_at.HasValue || na.updated_at == form.updated_at) &&
                (string.IsNullOrEmpty(form.parent_title) || na.parent_title == form.parent_title) &&
                (!form.children_count.HasValue || na.children_count == form.children_count) &&
                (string.IsNullOrEmpty(form.full_path) || na.full_path == form.full_path) &&
                (!form.is_parent.HasValue || na.is_parent == form.is_parent) &&
                (!form.is_leaf.HasValue || na.is_leaf == form.is_leaf))
            .ToList();
    }

    /// <summary>
    /// 查詢單一導航項目
    /// </summary>
    /// <param name="id">導航項目ID</param>
    public vw_sidebar_navigation? QueryNavigation(int? id)
        => ValidNavigations.FirstOrDefault(n => n.id == id);

    /// <summary>
    /// 查詢上層目錄列表（parent_code 為空代表根目錄）
    /// </summary>
    public List<vw_sidebar_navigation> QueryParentNavigationList()
        => ValidNavigations
            .Where(na => string.IsNullOrEmpty(na.parent_code))
            .ToList();

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