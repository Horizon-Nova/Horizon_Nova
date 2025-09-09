using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Repositories;

public class UserManagementRepositories(HnbHnbBackofficeDbContext hnb)
{
    #region 統一的查詢屬性
   
    private IQueryable<user_profile> ValidUserProfiles => hnb.user_profiles;

    public void UserProfileSave(user_profile model)
    {
        // 查詢舊資料
        var entity = hnb.user_profiles.FirstOrDefault(x => x.id == model.id);

        if (entity == null || model.id == 0)
        {
            hnb.user_profiles.Add(model);
        }
        else
        {
            entity.employee_name = model.employee_name;
            entity.email = model.email;
            entity.phone = model.phone;
            entity.avatar_path = model.avatar_path;
            entity.avatar_mime = model.avatar_mime;
            hnb.user_profiles.Update(entity);
        }

        hnb.SaveChanges();
    }

    #endregion
}
