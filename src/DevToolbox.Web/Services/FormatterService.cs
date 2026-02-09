using System.Text.Json;

namespace DevToolbox.Web.Services;

/// <summary>
/// Formatter service interface (AI Accept)
/// </summary>
public interface IFormatterService
{
    /// <summary>
    /// Format JSON with indentation (AI Accept)
    /// </summary>
    string JsonFormat(string input);

    /// <summary>
    /// Compress JSON by removing whitespace (AI Accept)
    /// </summary>
    string JsonCompress(string input);
}

/// <summary>
/// Formatter service implementation (AI Accept)
/// </summary>
public class FormatterService : IFormatterService
{
    // AI Accept [Added]: Shared serializer options
    private static readonly JsonSerializerOptions FormatOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions CompressOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Format JSON with indentation (AI Accept)
    /// </summary>
    public string JsonFormat(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty.");

        using var doc = JsonDocument.Parse(input, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        return JsonSerializer.Serialize(doc.RootElement, FormatOptions);
    }

    /// <summary>
    /// Compress JSON by removing all whitespace (AI Accept)
    /// </summary>
    public string JsonCompress(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty.");

        using var doc = JsonDocument.Parse(input, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        return JsonSerializer.Serialize(doc.RootElement, CompressOptions);
    }
}
