using HNB.Areas.HnbBackoffice.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Models.HnbHnbBackoffice;

namespace HNB.Areas.HnbBackoffice.Controllers;

[Area("HnbBackoffice")]
public class BackofficeController(DbKeyJwtService svc, IConfiguration cfg) : Controller
{
    public IActionResult Dashboard()
        => View();

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (req?.Username == "admin" && req?.Password == "123456")
        {
            var (token, exp) = await svc.IssueTokenAfterLoginAsync(
                ctx: HttpContext,
                keyComponents: req.Username,
                note: "登入產生",
                ct: ct);

            return Ok(new { ok = true, token, expires_at = exp });
        }

        return Unauthorized(new { ok = false, error = "invalid_credential" });
    }

    public record LoginRequest(string Username, string Password);


}
