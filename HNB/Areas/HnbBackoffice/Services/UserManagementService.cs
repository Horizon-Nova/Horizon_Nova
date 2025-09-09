using HNB.Areas.HnbBackoffice.Repositories;
using Models.HnbHnbBackoffice;
using System.Security.Cryptography.Xml;

namespace HNB.Areas.HnbBackoffice.Services;

public class UserManagementService(UserManagementRepositories rep, IWebHostEnvironment env)
{
    #region 存檔（可選：處理大頭照與密碼雜湊）
    public void SaveUserProfile(user_profile model, IFormFile? avatar)
    {
        if (avatar != null && avatar.Length > 0)
        {
            var ext = Path.GetExtension(avatar.FileName);
            var mime = avatar.ContentType;
            var root = Path.Combine(env.ContentRootPath, "storage", "avatars",
                                    DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"));
            Directory.CreateDirectory(root);
            var fname = $"{Guid.NewGuid():N}{ext}";
            var fpath = Path.Combine(root, fname);

            using var fs = new FileStream(fpath, FileMode.Create);
            avatar.CopyTo(fs);

            model.avatar_path = fpath.Replace(env.ContentRootPath, "").Replace('\\', '/');
            model.avatar_mime = mime;
        }

        rep.UserProfileSave(model);
    }
    #endregion
}
