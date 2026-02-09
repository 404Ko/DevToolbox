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
}
