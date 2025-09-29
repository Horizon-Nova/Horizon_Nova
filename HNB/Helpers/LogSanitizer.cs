namespace HNB.Helpers;

public static class LogSanitizer
{
    /// <summary>
    /// 清理輸入字串：移除 null bytes、控制字元
    /// </summary>
    public static string? Clean(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        var cleaned = input.Replace("\0", "");
        cleaned = new string(cleaned.Where(c => !char.IsControl(c)).ToArray());

        return cleaned;
    }
}