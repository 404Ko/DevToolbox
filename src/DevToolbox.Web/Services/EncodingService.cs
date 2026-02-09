using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DevToolbox.Web.Services;

/// <summary>
/// Encoding service interface (AI Accept)
/// </summary>
public interface IEncodingService
{
    string Base64Encode(string input);
    string Base64Decode(string input);
    string UrlEncode(string input);
    string UrlDecode(string input);
    string HtmlEncode(string input);
    string HtmlDecode(string input);
    string UnicodeEncode(string input);
    string UnicodeDecode(string input);
    string HexEncode(string input);
    string HexDecode(string input);
    string StripHtmlTags(string input);

    /// <summary>
    /// Strip HTML tags but convert block-level tags to newlines (AI Accept)
    /// </summary>
    string StripHtmlTagsKeepNewlines(string input);

    /// <summary>
    /// Get HTML content statistics (AI Accept)
    /// </summary>
    HtmlStripStats GetHtmlStats(string input);
}

/// <summary>
/// HTML strip statistics result (AI Accept)
/// </summary>
public class HtmlStripStats
{
    public int OriginalLength { get; set; }
    public int StrippedLength { get; set; }
    public int TagCount { get; set; }
    public int RemovedChars { get; set; }
    public Dictionary<string, int> TagBreakdown { get; set; } = new();
}

/// <summary>
/// Encoding service implementation (AI Accept)
/// </summary>
public class EncodingService : IEncodingService
{
    /// <summary>
    /// Base64 encode (AI Accept)
    /// </summary>
    public string Base64Encode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Base64 decode (AI Accept)
    /// </summary>
    public string Base64Decode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// URL encode (AI Accept)
    /// </summary>
    public string UrlEncode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return Uri.EscapeDataString(input);
    }

    /// <summary>
    /// URL decode (AI Accept)
    /// </summary>
    public string UrlDecode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return Uri.UnescapeDataString(input);
    }

    /// <summary>
    /// HTML encode (AI Accept)
    /// </summary>
    public string HtmlEncode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return HttpUtility.HtmlEncode(input);
    }

    /// <summary>
    /// HTML decode (AI Accept)
    /// </summary>
    public string HtmlDecode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return HttpUtility.HtmlDecode(input);
    }

    /// <summary>
    /// Unicode encode (\uXXXX format) (AI Accept)
    /// </summary>
    public string UnicodeEncode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var sb = new StringBuilder();
        foreach (char c in input)
        {
            if (c > 127)
                sb.Append($"\\u{(int)c:X4}");
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Unicode decode (AI Accept)
    /// </summary>
    public string UnicodeDecode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return Regex.Replace(input, @"\\u([0-9A-Fa-f]{4})", m =>
            ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
    }

    /// <summary>
    /// Hex encode (AI Accept)
    /// </summary>
    public string HexEncode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// Hex decode (AI Accept)
    /// </summary>
    public string HexDecode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var bytes = Convert.FromHexString(input);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Strip HTML tags (AI Accept)
    /// </summary>
    public string StripHtmlTags(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        // Remove HTML tags
        var result = Regex.Replace(input, "<[^>]*>", string.Empty);
        // Decode HTML entities
        result = HttpUtility.HtmlDecode(result);
        return result.Trim();
    }

    // AI Accept [Added]: Block-level tag regex for newline conversion
    private static readonly Regex BlockTagRegex = new(
        @"<\s*/?\s*(br|p|div|h[1-6]|li|tr|blockquote|hr|pre|section|article|header|footer|nav|aside|main|table|thead|tbody|tfoot|ul|ol|dl|dt|dd)\b[^>]*/?\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex AllTagRegex = new(
        @"<[^>]*>",
        RegexOptions.Compiled);

    private static readonly Regex TagNameRegex = new(
        @"<\s*/?(\w+)",
        RegexOptions.Compiled);

    private static readonly Regex MultiNewlineRegex = new(
        @"\n{3,}",
        RegexOptions.Compiled);

    /// <summary>
    /// Strip HTML tags but convert block-level tags to newlines (AI Accept)
    /// </summary>
    public string StripHtmlTagsKeepNewlines(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // AI Accept [Added]: Replace block-level tags with newline markers first
        var result = BlockTagRegex.Replace(input, "\n");

        // AI Accept [Added]: Remove remaining inline tags
        result = AllTagRegex.Replace(result, string.Empty);

        // AI Accept [Added]: Decode HTML entities
        result = HttpUtility.HtmlDecode(result);

        // AI Accept [Added]: Clean up excessive newlines
        result = MultiNewlineRegex.Replace(result, "\n\n");

        return result.Trim();
    }

    /// <summary>
    /// Get HTML content statistics (AI Accept)
    /// </summary>
    public HtmlStripStats GetHtmlStats(string input)
    {
        var stats = new HtmlStripStats();
        if (string.IsNullOrEmpty(input))
            return stats;

        stats.OriginalLength = input.Length;

        // AI Accept [Added]: Count all tags and build tag breakdown
        var tagMatches = TagNameRegex.Matches(input);
        stats.TagCount = tagMatches.Count;

        foreach (Match match in tagMatches)
        {
            var tagName = match.Groups[1].Value.ToLowerInvariant();
            if (stats.TagBreakdown.ContainsKey(tagName))
                stats.TagBreakdown[tagName]++;
            else
                stats.TagBreakdown[tagName] = 1;
        }

        // AI Accept [Added]: Calculate stripped length
        var stripped = StripHtmlTags(input);
        stats.StrippedLength = stripped.Length;
        stats.RemovedChars = stats.OriginalLength - stats.StrippedLength;

        return stats;
    }
}
