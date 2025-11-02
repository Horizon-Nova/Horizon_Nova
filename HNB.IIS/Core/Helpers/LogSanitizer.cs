namespace HNB.IIS.Core.Helpers;

public static class LogSanitizer
{
    public static string? Clean(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
            
        var cleaned = input.Replace("\0", "");
        cleaned = new string(cleaned.Where(c => !char.IsControl(c)).ToArray());
        
        return cleaned;
    }
}

