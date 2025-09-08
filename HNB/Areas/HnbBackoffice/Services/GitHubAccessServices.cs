using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
namespace HNB.Areas.HnbBackoffice.Services;

public class GitHubAccessServices
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubAccessServices> _logger;

    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _orgName;

    public GitHubAccessServices(IHttpContextAccessor accessor,IHttpClientFactory factory,IConfiguration config,ILogger<GitHubAccessServices> logger)
    {
        _httpContextAccessor = accessor;
        _httpClientFactory = factory;
        _logger = logger;

        _clientId = config["GitHubOAuth:ClientId"];
        _clientSecret = config["GitHubOAuth:ClientSecret"];
        _orgName = config["GitHubOAuth:OrgName"];
    }

    public async Task<bool> TrySignInFromCodeAsync(string code)
    {
        var accessToken = await GetAccessTokenAsync(code);
        if (string.IsNullOrEmpty(accessToken))
            return false;

        var userInfo = await GetUserInfoAsync(accessToken);
        if (string.IsNullOrEmpty(userInfo.Login))
            return false;

        _logger.LogInformation("GitHub login: {login}, name: {name}", userInfo.Login, userInfo.Name);

        var isOrgMember = await IsOrganizationMemberAsync(accessToken, userInfo.Login);
        if (!isOrgMember)
            return false;

        await SignInUserAsync(userInfo.Login, userInfo.Name, new[] { "Member" });
        return true;
    }

    private async Task<string> GetAccessTokenAsync(string code)
    {
        var client = _httpClientFactory.CreateClient();
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "code", code }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = new FormUrlEncodedContent(parameters)
        };
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var res = await client.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        return doc.RootElement.TryGetProperty("access_token", out var token)? token.GetString(): null;
    }

    private async Task<(string Login, string Name)> GetUserInfoAsync(string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        req.Headers.Add("Authorization", $"Bearer {accessToken}");
        req.Headers.Add("User-Agent", "HorizonNovaApp/1.0");

        var res = await client.SendAsync(req);
        if (!res.IsSuccessStatusCode) return default;

        var json = await res.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var login = doc.RootElement.GetProperty("login").GetString();
        var name = doc.RootElement.TryGetProperty("name", out var n) ? n.GetString() : login;

        return (login, name);
    }

    private async Task<bool> IsOrganizationMemberAsync(string accessToken, string login)
    {
        var client = _httpClientFactory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.github.com/orgs/{_orgName}/memberships/{login}");

        req.Headers.Add("Authorization", $"Bearer {accessToken}");
        req.Headers.Add("User-Agent", "HorizonNovaApp/1.0");
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        var res = await client.SendAsync(req);
        if (!res.IsSuccessStatusCode) return false;

        var json = await res.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("state", out var stateProp))
            return stateProp.GetString()?.Equals("active", StringComparison.OrdinalIgnoreCase) ?? false;

        return false;
    }

    public async Task SignInUserAsync(string login, string displayName, string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, displayName ?? login),
            new Claim(ClaimTypes.NameIdentifier, login)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await _httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal);
    }

    public async Task SignOutAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
            await context.SignOutAsync();
    }

}
