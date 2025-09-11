using Microsoft.EntityFrameworkCore;
using Models.HnbHnbBackoffice;
using System.Net;

namespace HNB.Areas.HnbBackoffice.Repositories;

public class AuthorizeRepositories(HnbHnbBackofficeDbContext dbo)
{
    #region 共同查詢
    private IQueryable<user_profile> ValidUserProfiles => dbo.user_profiles;
    #endregion

}
