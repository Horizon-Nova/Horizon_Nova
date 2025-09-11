using HNB.Areas.HnbBackoffice.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class TestController : Controller
{
    [HttpPost]
    public IActionResult CryptoToolTest(string account, string? salt)
    {
        var accRes = string.IsNullOrWhiteSpace(salt)
            ? CryptoToolUtilities.HashSha256ThenArgon2id(account)
            : CryptoToolUtilities.HashSha256ThenArgon2id(account, salt!);

        return Json(new
        {
            Tag = 1,
            Message = "ok",
            Data = new
            {
                input = new { account, salt = salt ?? "(generated)" },
                account_result = new { accRes.SaltBase64, accRes.HashBase64, accRes.Phc }
            }
        });
    }

}
