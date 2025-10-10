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
    
    /// <summary>
    /// 有效的側欄導航查詢來源
    /// </summary>
    public IQueryable<sidebar_navigation> ValidSidebarNavigations => db.sidebar_navigations;
    
    #endregion

    #region 專用查詢方法

    /// <summary>
    /// 查詢導航項目列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="parentCode">父項目篩選（null = 不過濾，空字串 = 只要根項目）</param>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<vw_sidebar_navigation> QueryNavigationList(string? searchTerm = null, string? parentCode = null, bool? isActive = null)
    {
        var query = ValidNavigations.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(n => 
                (n.title != null && n.title.Contains(searchTerm)) ||
                (n.code != null && n.code.Contains(searchTerm)));
        }
        
        if (parentCode != null)
        {
            if (parentCode == "")
            {
                query = query.Where(n => n.parent_code == null || n.parent_code == "");
            }
            else
            {
                query = query.Where(n => n.parent_code == parentCode);
            }
        }
        
        if (isActive.HasValue)
        {
            query = query.Where(n => n.is_active == isActive.Value);
        }
        
        return query.ToList();
    }

    /// <summary>
    /// 查詢單一導航項目
    /// </summary>
    /// <param name="id">導航項目ID</param>
    public vw_sidebar_navigation? QueryNavigation(int id)
        => ValidNavigations.FirstOrDefault(n => n.id == id);

    /// <summary>
    /// 查詢側欄導航項目列表
    /// </summary>
    /// <param name="searchTerm">搜尋關鍵字</param>
    /// <param name="parentCode">父項目篩選</param>
    /// <param name="isActive">啟用狀態篩選</param>
    public List<sidebar_navigation> QuerySidebarNavigationList( 
        string? searchTerm = null, string? parentCode = null, 
        bool? isActive = null)
        => ValidSidebarNavigations
            .Where(n =>
                (string.IsNullOrEmpty(searchTerm) || 
                    (n.title != null && n.title.Contains(searchTerm)) ||
                    (n.code != null && n.code.Contains(searchTerm))) &&
                (string.IsNullOrEmpty(parentCode) || n.parent_code == parentCode) &&
                (!isActive.HasValue || n.is_active == isActive.Value)
            )
            .ToList();

    /// <summary>
    /// 查詢單一側欄導航項目
    /// </summary>
    /// <param name="id">側欄導航項目ID</param>
    /// <param name="code">側欄導航代碼</param>
    public sidebar_navigation? QuerySidebarNavigation(int? id = null, string? code = null)
    {
        if (id.HasValue)
            return ValidSidebarNavigations.FirstOrDefault(n => n.id == id.Value);
        
        if (!string.IsNullOrEmpty(code))
            return ValidSidebarNavigations.FirstOrDefault(n => n.code == code);
        
        return null;
    }

    #endregion

    #region 基本 CRUD 操作
    
    /// <summary>
    /// 插入導航項目（新增或變更）
    /// </summary>
    public sidebar_navigation InsertNavigation(sidebar_navigation form)
    {
        var existingEntity = QuerySidebarNavigation(id: form.id);
        
        if (existingEntity == null)
        {
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
      
        db.SaveChanges();
        return existingEntity;
    }

    /// <summary>
    /// 更新導航項目
    /// </summary>
    public bool UpdateNavigation(sidebar_navigation form)
    {
        var existing = db.sidebar_navigations.Find(form.id);
        if (existing == null) return false;

        existing.title = form.title;
        existing.code = form.code;
        existing.url = form.url;
        existing.icon = form.icon;
        existing.parent_code = form.parent_code;
        existing.sort_order = form.sort_order;
        existing.is_active = form.is_active;
        existing.updated_at = DateTime.Now;

        return db.SaveChanges() > 0;
    }

    /// <summary>
    /// 刪除導航項目
    /// </summary>
    public bool DeleteNavigation(int id)
    {
        var entity = QuerySidebarNavigation(id: id);
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