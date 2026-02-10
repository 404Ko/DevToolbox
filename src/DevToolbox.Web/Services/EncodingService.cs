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

    /// <summary>
    /// Encode binary data to Base64 string (AI Accept)
    /// </summary>
    string Base64EncodeBytes(byte[] data);

    /// <summary>
    /// Decode Base64 string to binary data (AI Accept)
    /// </summary>
    byte[] Base64DecodeBytes(string base64);

    /// <summary>
    /// Detect MIME type from binary data magic bytes (AI Accept)
    /// </summary>
    string DetectMimeType(byte[] data);

    /// <summary>
    /// Get file extension from MIME type (AI Accept)
    /// </summary>
    string GetExtensionFromMime(string mimeType);
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

    /// <summary>
    /// Encode binary data to Base64 string (AI Accept)
    /// </summary>
    public string Base64EncodeBytes(byte[] data)
    {
        if (data == null || data.Length == 0) return string.Empty;
        return Convert.ToBase64String(data);
    }

    /// <summary>
    /// Decode Base64 string to binary data (AI Accept)
    /// </summary>
    public byte[] Base64DecodeBytes(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return Array.Empty<byte>();

        // AI Accept [Added]: Strip data URI prefix if present
        var cleaned = base64.Trim();
        var commaIdx = cleaned.IndexOf(',');
        if (commaIdx >= 0 && cleaned.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[(commaIdx + 1)..];
        }

        // AI Accept [Added]: Remove whitespace/newlines
        cleaned = Regex.Replace(cleaned, @"\s+", "");

        return Convert.FromBase64String(cleaned);
    }

    /// <summary>
    /// Detect MIME type from binary magic bytes (AI Accept)
    /// </summary>
    public string DetectMimeType(byte[] data)
    {
        if (data == null || data.Length < 4) return "application/octet-stream";

        // AI Accept [Added]: Check magic bytes for common file types
        if (data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            return "image/png";
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            return "image/jpeg";
        if (data.Length >= 6 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46)
            return "image/gif";
        if (data.Length >= 4 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46)
            return data.Length >= 12 && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50
                ? "image/webp" : "audio/wav";
        if (data.Length >= 4 && data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46)
            return "application/pdf";
        if (data.Length >= 4 && data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04)
            return "application/zip";
        if (data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B)
            return "application/gzip";
        if (data.Length >= 4 && data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x00 &&
            (data[3] == 0x18 || data[3] == 0x1C || data[3] == 0x20))
            return "video/mp4";
        if (data.Length >= 4 && data[0] == 0x49 && data[1] == 0x44 && data[2] == 0x33)
            return "audio/mpeg";
        if (data.Length >= 4 && data[0] == 0x4F && data[1] == 0x67 && data[2] == 0x67 && data[3] == 0x53)
            return "audio/ogg";
        if (data.Length >= 2 && data[0] == 0x42 && data[1] == 0x4D)
            return "image/bmp";
        if (data.Length >= 4 && data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x01 && data[3] == 0x00)
            return "image/x-icon";

        // AI Accept [Added]: Check text-based formats
        var header = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 256));
        if (header.TrimStart().StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
            return "image/svg+xml";
        if (header.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
            header.TrimStart().StartsWith("<", StringComparison.OrdinalIgnoreCase))
            return "application/xml";
        if (header.TrimStart().StartsWith("{") || header.TrimStart().StartsWith("["))
            return "application/json";

        return "application/octet-stream";
    }

    /// <summary>
    /// Get file extension from MIME type (AI Accept)
    /// </summary>
    public string GetExtensionFromMime(string mimeType) => mimeType switch
    {
        "image/png" => ".png",
        "image/jpeg" => ".jpg",
        "image/gif" => ".gif",
        "image/webp" => ".webp",
        "image/bmp" => ".bmp",
        "image/svg+xml" => ".svg",
        "image/x-icon" => ".ico",
        "application/pdf" => ".pdf",
        "application/zip" => ".zip",
        "application/gzip" => ".gz",
        "application/json" => ".json",
        "application/xml" => ".xml",
        "audio/mpeg" => ".mp3",
        "audio/wav" => ".wav",
        "audio/ogg" => ".ogg",
        "video/mp4" => ".mp4",
        _ => ".bin"
    };
}
