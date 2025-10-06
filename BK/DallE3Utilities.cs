using ManagementSystem.Repositories;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ManagementSystem.Utilities;

public sealed class DallE3Utilities(HttpClient http, DallE3Repositories rep)
{
    #region const
    private const string Scope = "image.general";
    private const string ModelKey = "gpt-image-1";
    private const string OutputUrlRel = "/storage/image/TempFiles";
    private const int MaxEdge = 1024;
    private const int JpegQuality = 82;
    #endregion

    #region type
    private sealed record EditResult(bool Success, int ElapsedMs, decimal TotalCost, int OutCount, string OutputB64, string RawJson);
    private sealed record DalleCfg(long SettingId, string BaseUrl, string ApiKey, string Model, string Org, string SizeOpt, string Prompt);
    #endregion

    #region 讀設定

    private static string ReadS(JsonDocument j, string k)
        => j.RootElement.GetProperty(k).GetString()!;

    private async Task<DalleCfg> LoadCfgAsync(string? promptOverride, CancellationToken ct)
    {
        var setting = await rep.GetAiSettingAsync(Scope, ModelKey).ConfigureAwait(false);
        using var cfg = JsonDocument.Parse(setting!.config);
        using var tmpl = JsonDocument.Parse(setting.prompt_templates);

        var prompt = promptOverride ?? tmpl.RootElement.GetProperty("ProductImageDefault").GetString()!;
        return new DalleCfg(
            setting.id,
            ReadS(cfg, "base_url"),
            ReadS(cfg, "api_key"),
            ReadS(cfg, "image_model"),
            cfg.RootElement.TryGetProperty("organization", out var org) && org.ValueKind == JsonValueKind.String ? org.GetString()! : "",
            ReadS(cfg, "size"),
            prompt
        );
    }

    #endregion

    #region 單張編輯
    private async Task<EditResult> DallEImageTaskAsync(string baseImageBase64, DalleCfg c, CancellationToken ct = default)
    {
        var imgBytes = Convert.FromBase64String(baseImageBase64);

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{c.BaseUrl.TrimEnd('/')}/images/edits") { Version = new Version(2, 0) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.ApiKey);
        req.Headers.ExpectContinue = false;
        if (!string.IsNullOrEmpty(c.Org)) req.Headers.Add("OpenAI-Organization", c.Org);

        var mp = new MultipartFormDataContent();
        var basePart = new ByteArrayContent(imgBytes);
        basePart.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        mp.Add(basePart, "image", "base.jpg");
        mp.Add(new StringContent(c.Model), "model");
        mp.Add(new StringContent(c.SizeOpt), "size");
        mp.Add(new StringContent(c.Prompt), "prompt");
        req.Content = mp;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        sw.Stop();

        var success = false;
        var outCount = 0;
        var outputB64 = "";
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
            {
                outCount = dataEl.GetArrayLength();
                outputB64 = outCount > 0 ? (dataEl[0].TryGetProperty("b64_json", out var bz) ? (bz.GetString() ?? "") : "") : "";
                success = !string.IsNullOrEmpty(outputB64) ? true : false;
            }
            else success = false;
        }
        catch { success = false; outputB64 = ""; outCount = 0; }

        decimal unitCost =
            c.SizeOpt == "1024x1024" ? 0.04m :
            c.SizeOpt == "512x512" ? 0.02m :
            0.03m;
        var totalCost = success ? (outCount > 0 ? unitCost * outCount : unitCost) : 0m;

        return new EditResult(success, (int)sw.ElapsedMilliseconds, totalCost, outCount, outputB64, raw);
    }
    #endregion

    public async Task<(bool ok, string urlsText)> ProcessImagesAsync(IFormFile[] images, CancellationToken ct = default)
    {
        var pick = images?.Take(5).ToArray() ?? Array.Empty<IFormFile>();
        if (pick.Length == 0) return (false, "");

        var cfg = await LoadCfgAsync(null, ct).ConfigureAwait(false);

        var b64List = new List<string>(pick.Length);
        foreach (var f in pick)
        {
            await using var ms = new MemoryStream();
            await f.CopyToAsync(ms, ct).ConfigureAwait(false);
            ms.Position = 0;
            var b64 = await ManagementSystem.Utilities.ImageUtilities.ToJpegBase64Async(ms, MaxEdge, JpegQuality, ct).ConfigureAwait(false);
            b64List.Add(b64);
        }

        var results = await Task.WhenAll(b64List.Select(b64 => DallEImageTaskAsync(b64, cfg, ct))).ConfigureAwait(false);

        var urlList = new List<string>(results.Length * 5);
        foreach (var r in results)
        {
            var status = r.Success ? "success" : "error";
            var id = Guid.NewGuid().ToString("N");
            var urlRel = $"{OutputUrlRel}/{id}.png".Replace("\\", "/");

            if (r.Success)
            {
                _ = await ManagementSystem.Utilities.ImageUtilities.SaveBase64AsPngAsync(r.OutputB64, urlRel, ct).ConfigureAwait(false);
                urlList.Add($"storage {urlRel}");

                var pieces = await ManagementSystem.Utilities.ImageUtilities.CropGridAsync(urlRel, 2, 2, ct).ConfigureAwait(false);
                foreach (var p in pieces)
                    urlList.Add($"storage {p.UrlRel}");
            }

            var meta = JsonSerializer.Serialize(new { endpoint = "images/edits", size = cfg.SizeOpt, model = cfg.Model, codec = "jpeg", maxEdge = MaxEdge, q = JpegQuality });
            await rep.InsertUsageLogAsync(cfg.SettingId, status, r.TotalCost, null, r.ElapsedMs, meta, r.Success ? urlRel : null).ConfigureAwait(false);
        }

        var ok = urlList.Count > 0 ? true : false;
        var urlsText = string.Join("\n", urlList);
        return (ok, urlsText);
    }

}
