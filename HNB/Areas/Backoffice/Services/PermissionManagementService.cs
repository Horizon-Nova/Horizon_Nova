using HNB.Areas.Backoffice.Repositories;
using Models.HnbHnbBackoffice;
using System.Security.Cryptography;
using System.Text;

namespace HNB.Areas.Backoffice.Services;

public class PermissionManagementService(PermissionManagementRepository repo, SidebarNavigationService sidebarService)
{
    #region 統一的查詢來源 
    /// <summary> User清單 </summary>
    public List<vw_permission_user> LoadUsers(int id)
        => repo.QueryUserList(id);

    /// <summary> User單筆 </summary>
    public vw_permission_user? LoadUserById(int id)
        => repo.QueryUser(id);

    #endregion

}