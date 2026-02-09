using System.Text;
using System.Text.Json;

namespace DevToolbox.Web.Services;

/// <summary>
/// JWT parsing service interface (AI Accept)
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Parse JWT token and return structured result (AI Accept)
    /// </summary>
    JwtParseResult Parse(string token);
}

/// <summary>
/// JWT parse result model (AI Accept)
/// </summary>
public class JwtParseResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string HeaderJson { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public List<JwtClaim> Claims { get; set; } = new();
    public DateTime? IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? NotBefore { get; set; }
    public bool IsExpired { get; set; }
}

/// <summary>
/// JWT claim key-value pair (AI Accept)
/// </summary>
public class JwtClaim
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// JWT parsing service implementation (AI Accept)
/// </summary>
public class JwtService : IJwtService
{
    // AI Accept [Added]: Well-known JWT claim descriptions
    private static readonly Dictionary<string, string> ClaimDescriptions = new()
    {
        ["iss"] = "Issuer (签发者)",
        ["sub"] = "Subject (主题)",
        ["aud"] = "Audience (受众)",
        ["exp"] = "Expiration Time (过期时间)",
        ["nbf"] = "Not Before (生效时间)",
        ["iat"] = "Issued At (签发时间)",
        ["jti"] = "JWT ID (唯一标识)",
        ["name"] = "Name (姓名)",
        ["email"] = "Email (邮箱)",
        ["role"] = "Role (角色)",
        ["roles"] = "Roles (角色列表)",
        ["scope"] = "Scope (权限范围)",
        ["azp"] = "Authorized Party (授权方)",
        ["typ"] = "Token Type (令牌类型)",
        ["nonce"] = "Nonce (随机数)",
        ["sid"] = "Session ID (会话ID)",
    };

    /// <summary>
    /// Parse JWT token into header, payload and signature (AI Accept)
    /// </summary>
    public JwtParseResult Parse(string token)
    {
        var result = new JwtParseResult();

        if (string.IsNullOrWhiteSpace(token))
        {
            result.ErrorMessage = "JWT token cannot be empty";
            return result;
        }

        // AI Accept [Added]: Strip "Bearer " prefix if present
        token = token.Trim();
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token["Bearer ".Length..].Trim();
        }

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            result.ErrorMessage = $"Invalid JWT format: expected 3 parts separated by '.', got {parts.Length}";
            return result;
        }

        try
        {
            // AI Accept [Added]: Decode header
            var headerJson = Base64UrlDecode(parts[0]);
            result.HeaderJson = FormatJson(headerJson);
            ParseHeader(headerJson, result);

            // AI Accept [Added]: Decode payload
            var payloadJson = Base64UrlDecode(parts[1]);
            result.PayloadJson = FormatJson(payloadJson);
            ParsePayload(payloadJson, result);

            // AI Accept [Added]: Signature as Base64
            result.Signature = parts[2];

            result.IsValid = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Failed to parse JWT: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Base64Url decode to UTF-8 string (AI Accept)
    /// </summary>
    private static string Base64UrlDecode(string input)
    {
        // AI Accept [Added]: Convert Base64Url to standard Base64
        var base64 = input
            .Replace('-', '+')
            .Replace('_', '/');

        // Pad to multiple of 4
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Format JSON string with indentation (AI Accept)
    /// </summary>
    private static string FormatJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Parse JWT header for algorithm and type (AI Accept)
    /// </summary>
    private static void ParseHeader(string headerJson, JwtParseResult result)
    {
        try
        {
            using var doc = JsonDocument.Parse(headerJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("alg", out var alg))
                result.Algorithm = alg.GetString() ?? string.Empty;

            if (root.TryGetProperty("typ", out var typ))
                result.TokenType = typ.GetString() ?? string.Empty;
        }
        catch
        {
            // Header parse failed, not critical
        }
    }

    /// <summary>
    /// Parse JWT payload for claims and timestamps (AI Accept)
    /// </summary>
    private static void ParsePayload(string payloadJson, JwtParseResult result)
    {
        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            foreach (var prop in root.EnumerateObject())
            {
                var claim = new JwtClaim
                {
                    Key = prop.Name,
                    Value = prop.Value.ValueKind == JsonValueKind.String
                        ? prop.Value.GetString() ?? string.Empty
                        : prop.Value.GetRawText(),
                    Description = ClaimDescriptions.GetValueOrDefault(prop.Name, string.Empty)
                };
                result.Claims.Add(claim);
            }

            // AI Accept [Added]: Parse timestamp claims
            if (root.TryGetProperty("exp", out var exp) && exp.TryGetInt64(out var expVal))
            {
                result.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expVal).LocalDateTime;
                result.IsExpired = DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expVal;
            }

            if (root.TryGetProperty("iat", out var iat) && iat.TryGetInt64(out var iatVal))
            {
                result.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(iatVal).LocalDateTime;
            }

            if (root.TryGetProperty("nbf", out var nbf) && nbf.TryGetInt64(out var nbfVal))
            {
                result.NotBefore = DateTimeOffset.FromUnixTimeSeconds(nbfVal).LocalDateTime;
            }
        }
        catch
        {
            // Payload parse failed, not critical
        }
    }
}
