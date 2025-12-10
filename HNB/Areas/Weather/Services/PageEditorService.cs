using System.Text;

namespace HNB.Areas.Weather.Services;

/// <summary>
/// 頁面編輯服務，負責處理檔案讀寫功能
/// </summary>
public class PageEditorService
{
    private readonly string _viewsPath;
    private readonly ILogger<PageEditorService> _logger;

    public PageEditorService(IWebHostEnvironment env, ILogger<PageEditorService> logger)
    {
        _viewsPath = Path.Combine(env.ContentRootPath, "Areas", "Weather", "Views");
        _logger = logger;
    }

    /// <summary>
    /// 讀取檔案內容
    /// </summary>
    /// <param name="pageName">頁面名稱（Outfit/Entry/Profile/Test）</param>
    /// <param name="sectionName">區塊名稱（head/middle/bottom/Main/Styles 等）</param>
    /// <returns>檔案內容，如果檔案不存在則返回 null</returns>
    public string? LoadFileContent(string pageName, string sectionName)
    {
        var filePath = GetFilePath(pageName, sectionName);
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("檔案不存在: {FilePath}", filePath);
            return null;
        }

        try
        {
            return File.ReadAllText(filePath, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "讀取檔案失敗: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 儲存檔案內容
    /// </summary>
    /// <param name="pageName">頁面名稱</param>
    /// <param name="sectionName">區塊名稱</param>
    /// <param name="content">檔案內容</param>
    /// <returns>是否儲存成功</returns>
    public bool SaveFileContent(string pageName, string sectionName, string content)
    {
        var filePath = GetFilePath(pageName, sectionName);
        
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);
            _logger.LogInformation("檔案儲存成功: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存檔案失敗: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 取得檔案路徑
    /// </summary>
    private string GetFilePath(string pageName, string sectionName)
    {
        var fileName = sectionName.StartsWith("_") ? sectionName : $"_{sectionName}";
        return Path.Combine(_viewsPath, pageName, "Partials", $"{fileName}.cshtml");
    }

    /// <summary>
    /// 取得可編輯的頁面列表
    /// </summary>
    public List<string> GetEditablePages()
    {
        var pages = new List<string>();
        var pagesPath = Path.Combine(_viewsPath);
        
        if (!Directory.Exists(pagesPath))
        {
            return pages;
        }

        var directories = Directory.GetDirectories(pagesPath);
        foreach (var dir in directories)
        {
            var pageName = Path.GetFileName(dir);
            var partialsPath = Path.Combine(dir, "Partials");
            if (Directory.Exists(partialsPath))
            {
                pages.Add(pageName);
            }
        }

        return pages.OrderBy(p => p).ToList();
    }

    /// <summary>
    /// 取得指定頁面的可編輯區塊列表
    /// </summary>
    public List<string> GetEditableSections(string pageName)
    {
        var sections = new List<string>();
        var partialsPath = Path.Combine(_viewsPath, pageName, "Partials");
        
        if (!Directory.Exists(partialsPath))
        {
            return sections;
        }

        var files = Directory.GetFiles(partialsPath, "*.cshtml");
        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName.StartsWith("_"))
            {
                sections.Add(fileName.Substring(1));
            }
        }

        return sections.OrderBy(s => s).ToList();
    }

    /// <summary>
    /// 取得區塊的友好顯示名稱
    /// </summary>
    public string GetSectionDisplayName(string sectionName)
    {
        var displayNames = new Dictionary<string, string>
        {
            { "head", "標題區塊" },
            { "middle", "中間區塊" },
            { "bottom", "底部區塊" },
            { "Styles", "樣式設定" },
            { "Main", "主要內容" },
            { "Content", "內容區塊" }
        };

        return displayNames.TryGetValue(sectionName, out var displayName) 
            ? displayName 
            : sectionName;
    }

    /// <summary>
    /// 解析 Partial 檔案，找出可編輯元素
    /// </summary>
    public List<EditableElement> GetEditableElements(string pageName, string sectionName)
    {
        var elements = new List<EditableElement>();
        var content = LoadFileContent(pageName, sectionName);
        
        if (string.IsNullOrEmpty(content))
        {
            return elements;
        }

        // 解析文字元素（h1, h2, h3, p 等）
        ParseTextElements(content, elements);
        
        // 解析按鈕元素
        ParseButtonElements(content, elements);
        
        // 解析背景圖片（在 Styles 中）
        if (sectionName == "Styles")
        {
            ParseBackgroundImages(content, elements);
        }

        return elements;
    }

    private void ParseTextElements(string content, List<EditableElement> elements)
    {
        var textPatterns = new[]
        {
            (@"<h1[^>]*>(.*?)</h1>", "標題 1"),
            (@"<h2[^>]*>(.*?)</h2>", "標題 2"),
            (@"<h3[^>]*>(.*?)</h3>", "標題 3"),
            (@"<p[^>]*>(.*?)</p>", "段落")
        };

        foreach (var (pattern, type) in textPatterns)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(content, pattern, System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var text = System.Text.RegularExpressions.Regex.Replace(match.Groups[1].Value, @"<[^>]+>", "").Trim();
                if (!string.IsNullOrEmpty(text) && text.Length < 200)
                {
                    elements.Add(new EditableElement
                    {
                        Id = $"text_{type}_{i}",
                        Name = $"{type} {i + 1}: {text.Substring(0, Math.Min(30, text.Length))}...",
                        Type = "text",
                        Selector = $"h1:nth-of-type({i + 1}), h2:nth-of-type({i + 1}), h3:nth-of-type({i + 1}), p:nth-of-type({i + 1})",
                        Value = text
                    });
                }
            }
        }
    }

    private void ParseButtonElements(string content, List<EditableElement> elements)
    {
        // 解析超連結（<a> 標籤）
        var linkPattern = @"<a[^>]*>(.*?)</a>";
        var linkMatches = System.Text.RegularExpressions.Regex.Matches(content, linkPattern, System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        for (int i = 0; i < linkMatches.Count; i++)
        {
            var match = linkMatches[i];
            var text = System.Text.RegularExpressions.Regex.Replace(match.Groups[1].Value, @"<[^>]+>", "").Trim();
            var hrefMatch = System.Text.RegularExpressions.Regex.Match(match.Value, @"href=[""']([^""']+)[""']");
            var href = hrefMatch.Success ? hrefMatch.Groups[1].Value : "";
            
            if (!string.IsNullOrEmpty(text))
            {
                elements.Add(new EditableElement
                {
                    Id = $"link_{i}",
                    Name = $"超連結 {i + 1}: {text}",
                    Type = "link",
                    Selector = $"a:nth-of-type({i + 1})",
                    Value = text,
                    ExtraData = href
                });
            }
        }
        
        // 解析按鈕（<button> 標籤，使用 onclick）
        var buttonPattern = @"<button[^>]*>(.*?)</button>";
        var buttonMatches = System.Text.RegularExpressions.Regex.Matches(content, buttonPattern, System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        for (int i = 0; i < buttonMatches.Count; i++)
        {
            var match = buttonMatches[i];
            var text = System.Text.RegularExpressions.Regex.Replace(match.Groups[1].Value, @"<[^>]+>", "").Trim();
            var onclickMatch = System.Text.RegularExpressions.Regex.Match(match.Value, @"onclick=[""']([^""']+)[""']");
            var onclick = onclickMatch.Success ? onclickMatch.Groups[1].Value : "";
            
            if (!string.IsNullOrEmpty(text))
            {
                elements.Add(new EditableElement
                {
                    Id = $"button_{i}",
                    Name = $"按鈕 {i + 1}: {text}",
                    Type = "button",
                    Selector = $"button:nth-of-type({i + 1})",
                    Value = text,
                    ExtraData = onclick
                });
            }
        }
    }

    private void ParseBackgroundImages(string content, List<EditableElement> elements)
    {
        var bgPattern = @"background[^:]*:\s*url\(['""]?([^'""\)]+)['""]?\)";
        var matches = System.Text.RegularExpressions.Regex.Matches(content, bgPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var url = match.Groups[1].Value;
            
            // 從匹配位置往前找，找到最接近的選擇器（從後往前找最後一個匹配的）
            var beforeMatch = content.Substring(0, match.Index);
            var selectorMatches = System.Text.RegularExpressions.Regex.Matches(beforeMatch, @"#([a-z0-9-]+)\s*\{", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var selector = "";
            
            if (selectorMatches.Count > 0)
            {
                // 取最後一個匹配的選擇器（最接近背景圖片的那個）
                var lastMatch = selectorMatches[selectorMatches.Count - 1];
                selector = $"#{lastMatch.Groups[1].Value}";
            }
            
            elements.Add(new EditableElement
            {
                Id = $"background_{i}",
                Name = $"背景圖片 {i + 1}: {Path.GetFileName(url)}",
                Type = "background",
                Selector = selector,
                Value = url
            });
        }
    }
}

/// <summary>
/// 可編輯元素模型
/// </summary>
public class EditableElement
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Selector { get; set; } = "";
    public string Value { get; set; } = "";
    public string ExtraData { get; set; } = "";
}

