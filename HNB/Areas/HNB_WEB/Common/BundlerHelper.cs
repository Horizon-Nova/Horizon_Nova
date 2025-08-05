using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;

namespace HNB.Areas.HNB_WEB.Common;

/// <summary>
/// Adapted from https://github.com/madskristensen/BundlerMinifier/wiki/Unbundling-scripts-for-debugging
/// 
/// Areas modified:
///  - Modified it to make it work with aspnetcore.
///  - Accept both scripts and styles.
///  - Read config from wwwroot
///  - Accept baseFolder since DI not suited for static methods
///  - Style nitpicks
/// </summary>
public class BundlerHelper
{
    private static long version = DateTime.Now.Ticks;

    public static HtmlString Render(string baseFolder, string bundlePath)
    {
        if (string.IsNullOrWhiteSpace(bundlePath))
            throw new ArgumentException("bundlePath 不可為空");

        string normalizedPath = bundlePath
            .TrimStart('~', '/');

        string configFile = Path.Combine(baseFolder, "bundleconfig.json");

        var bundle = GetBundle(configFile, normalizedPath);

        List<string> tags;

        if (bundle != null)
        {
            tags = normalizedPath.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                ? bundle.InputFiles.Select(inputFile => $"<script src='/{inputFile}?v={version}' type='text/javascript'></script>").ToList()
                : bundle.InputFiles.Select(inputFile => $"<link rel='stylesheet' href='/{inputFile}?v={version}' />").ToList();
        }
        else
        {
            tags = normalizedPath.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                ? new List<string> { $"<script src='/{normalizedPath}?v={version}' type='text/javascript'></script>" }
                : new List<string> { $"<link rel='stylesheet' href='/{normalizedPath}?v={version}' />" };
        }

        return new HtmlString(string.Join("\n", tags));
    }

    private static Bundle? GetBundle(string configFile, string bundlePath)
    {
        if (!File.Exists(configFile))
            return null;

        var json = File.ReadAllText(configFile);
        var bundles = JsonConvert.DeserializeObject<List<Bundle>>(json);

        return bundles?.FirstOrDefault(b =>
            b.OutputFileName?.TrimStart('~', '/').EndsWith(bundlePath, StringComparison.OrdinalIgnoreCase) == true);
    }

    class Bundle
    {
        [JsonProperty("outputFileName")]
        public string OutputFileName { get; set; } = "";

        [JsonProperty("inputFiles")]
        public List<string> InputFiles { get; set; } = new();
    }
}
