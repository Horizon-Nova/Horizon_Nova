using HNB.Areas.HnbBackoffice.Repositories;
using HNB.Areas.HnbBackoffice.Utilities;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;

namespace HNB.Areas.HnbBackoffice.Services;

public class UserManagementService(UserManagementRepositories rep, IWebHostEnvironment env)
{
    #region 主畫面

    /// <summary> 存檔 </summary>
    public void SaveUserProfile(user_profile model, IFormFile? avatar)
    {

        if (!string.IsNullOrWhiteSpace(model.account))
        {
            var accRes = CryptoToolUtilities.HashSha256ThenArgon2id(model.account);
            model.account = accRes.Phc;
            model.salts = new List<string> { accRes.SaltBase64 };
        }

        if (!string.IsNullOrWhiteSpace(model.password))
        {
            var pwdRes = CryptoToolUtilities.HashSha256ThenArgon2id(model.password);
            model.password = pwdRes.Phc;
            model.salts ??= new List<string>();
            model.salts.Add(pwdRes.SaltBase64);
        }

        if (avatar is { Length: > 0 })
        {
            var ext = Path.GetExtension(avatar.FileName);
            var root = Path.Combine(
                env.ContentRootPath,
                "Areas", "HnbBackoffice", "storage",
                DateTime.UtcNow.ToString("yyyy"),
                DateTime.UtcNow.ToString("MM")
            );
            Directory.CreateDirectory(root);

            var fname = $"{Guid.NewGuid():N}{ext}";
            var fpath = Path.Combine(root, fname);

            using var fs = new FileStream(fpath, FileMode.Create);
            avatar.CopyTo(fs);
        }

        rep.UpdateUserProfile(model);
    }

    #endregion

    #region users (用戶管理)

    public List<person_relation_v> QueryUsersResult(person_relation_v model)
    {
        return rep.QueryPersonrelation(model);
    }

    #endregion
}
