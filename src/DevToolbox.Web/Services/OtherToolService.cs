using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DevToolbox.Web.Services;

#region Data Models

/// <summary>
/// Regex match result item (AI Accept)
/// </summary>
public class RegexMatchItem
{
    public int Index { get; set; }
    public string Value { get; set; } = string.Empty;
    public int Length { get; set; }
    public List<RegexGroupItem> Groups { get; set; } = new();
}

/// <summary>
/// Regex group item (AI Accept)
/// </summary>
public class RegexGroupItem
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Regex test result (AI Accept)
/// </summary>
public class RegexTestResult
{
    public bool Success { get; set; }
    public int MatchCount { get; set; }
    public string? ErrorMessage { get; set; }
    public List<RegexMatchItem> Matches { get; set; } = new();
    public string? ReplaceResult { get; set; }
}

/// <summary>
/// Radix conversion result (AI Accept)
/// </summary>
public class RadixConvertResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string Binary { get; set; } = string.Empty;
    public string Octal { get; set; } = string.Empty;
    public string Decimal { get; set; } = string.Empty;
    public string Hex { get; set; } = string.Empty;
}

/// <summary>
/// Color conversion result (AI Accept)
/// </summary>
public class ColorConvertResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string Hex { get; set; } = string.Empty;
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
    public double H { get; set; }
    public double S { get; set; }
    public double L { get; set; }
    public string Rgb => $"rgb({R}, {G}, {B})";
    public string Hsl => $"hsl({H:F0}, {S:F1}%, {L:F1}%)";
}

/// <summary>
/// Character statistics result (AI Accept)
/// </summary>
public class CharStatsResult
{
    public int TotalChars { get; set; }
    public int TotalCharsNoSpaces { get; set; }
    public int WordCount { get; set; }
    public int LineCount { get; set; }
    public int LetterCount { get; set; }
    public int DigitCount { get; set; }
    public int SpaceCount { get; set; }
    public int PunctuationCount { get; set; }
    public int ChineseCount { get; set; }
    public int ByteCountUtf8 { get; set; }
    public int ByteCountAscii { get; set; }
    public List<CharFrequency> TopFrequencies { get; set; } = new();
}

/// <summary>
/// Character frequency item (AI Accept)
/// </summary>
public class CharFrequency
{
    public string Char { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percent { get; set; }
}

#endregion

/// <summary>
/// Other tools service interface (AI Accept)
/// </summary>
public interface IOtherToolService
{
    /// <summary>
    /// Test regex pattern against input (AI Accept)
    /// </summary>
    RegexTestResult TestRegex(string pattern, string input, bool ignoreCase, bool multiline, bool singleline, bool global, string? replacement);

    /// <summary>
    /// Convert number between radixes (AI Accept)
    /// </summary>
    RadixConvertResult ConvertRadix(string value, int fromBase);

    /// <summary>
    /// Parse color from HEX string (AI Accept)
    /// </summary>
    ColorConvertResult ParseHex(string hex);

    /// <summary>
    /// Parse color from RGB values (AI Accept)
    /// </summary>
    ColorConvertResult ParseRgb(int r, int g, int b);

    /// <summary>
    /// Parse color from HSL values (AI Accept)
    /// </summary>
    ColorConvertResult ParseHsl(double h, double s, double l);

    /// <summary>
    /// Compute character statistics (AI Accept)
    /// </summary>
    CharStatsResult GetCharStats(string text);
}

/// <summary>
/// Other tools service implementation (AI Accept)
/// </summary>
public class OtherToolService : IOtherToolService
{
    /// <summary>
    /// Test regex pattern against input text (AI Accept)
    /// </summary>
    public RegexTestResult TestRegex(string pattern, string input, bool ignoreCase, bool multiline, bool singleline, bool global, string? replacement)
    {
        var result = new RegexTestResult();

        if (string.IsNullOrEmpty(pattern))
        {
            result.ErrorMessage = "Pattern cannot be empty";
            return result;
        }

        try
        {
            // AI Accept [Added]: Build regex options from flags
            var options = RegexOptions.None;
            if (ignoreCase) options |= RegexOptions.IgnoreCase;
            if (multiline) options |= RegexOptions.Multiline;
            if (singleline) options |= RegexOptions.Singleline;

            var regex = new Regex(pattern, options, TimeSpan.FromSeconds(5));
            var matches = regex.Matches(input ?? string.Empty);

            // AI Accept [Added]: Collect match results
            var matchList = global ? matches.ToList() : (matches.Count > 0 ? new List<Match> { matches[0] } : new List<Match>());

            foreach (var match in matchList)
            {
                var item = new RegexMatchItem
                {
                    Index = match.Index,
                    Value = match.Value,
                    Length = match.Length
                };

                // AI Accept [Added]: Collect named/numbered groups
                for (int g = 1; g < match.Groups.Count; g++)
                {
                    var group = match.Groups[g];
                    item.Groups.Add(new RegexGroupItem
                    {
                        Name = regex.GroupNameFromNumber(g) != g.ToString() ? regex.GroupNameFromNumber(g) : $"#{g}",
                        Value = group.Value
                    });
                }

                result.Matches.Add(item);
            }

            result.MatchCount = result.Matches.Count;
            result.Success = result.MatchCount > 0;

            // AI Accept [Added]: Replacement if provided
            if (replacement != null)
            {
                result.ReplaceResult = global
                    ? regex.Replace(input ?? string.Empty, replacement)
                    : regex.Replace(input ?? string.Empty, replacement, 1);
            }
        }
        catch (RegexParseException ex)
        {
            result.ErrorMessage = $"Invalid regex: {ex.Message}";
        }
        catch (RegexMatchTimeoutException)
        {
            result.ErrorMessage = "Regex execution timed out (>5s)";
        }

        return result;
    }

    /// <summary>
    /// Convert a number string between binary/octal/decimal/hex (AI Accept)
    /// </summary>
    public RadixConvertResult ConvertRadix(string value, int fromBase)
    {
        var result = new RadixConvertResult();

        if (string.IsNullOrWhiteSpace(value))
        {
            result.ErrorMessage = "Input cannot be empty";
            return result;
        }

        try
        {
            // AI Accept [Added]: Parse input to long, then convert to all bases
            var cleaned = value.Trim().Replace(" ", "").Replace("_", "");

            // Remove common prefixes
            if (fromBase == 16 && cleaned.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned[2..];
            if (fromBase == 2 && cleaned.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned[2..];
            if (fromBase == 8 && cleaned.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned[2..];

            long number = Convert.ToInt64(cleaned, fromBase);

            result.Binary = Convert.ToString(number, 2);
            result.Octal = Convert.ToString(number, 8);
            result.Decimal = number.ToString();
            result.Hex = Convert.ToString(number, 16).ToUpper();
            result.Success = true;
        }
        catch (FormatException)
        {
            result.ErrorMessage = $"Invalid number for base-{fromBase}: \"{value}\"";
        }
        catch (OverflowException)
        {
            result.ErrorMessage = "Number too large (exceeds Int64 range)";
        }
        catch (ArgumentException ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Parse HEX color string to all formats (AI Accept)
    /// </summary>
    public ColorConvertResult ParseHex(string hex)
    {
        var result = new ColorConvertResult();

        if (string.IsNullOrWhiteSpace(hex))
        {
            result.ErrorMessage = "HEX value cannot be empty";
            return result;
        }

        // AI Accept [Added]: Normalize hex input
        var h = hex.Trim().TrimStart('#');

        // Support 3-char shorthand (#FFF → #FFFFFF)
        if (h.Length == 3)
            h = $"{h[0]}{h[0]}{h[1]}{h[1]}{h[2]}{h[2]}";

        if (h.Length != 6 || !int.TryParse(h, NumberStyles.HexNumber, null, out _))
        {
            result.ErrorMessage = $"Invalid HEX color: \"{hex}\"";
            return result;
        }

        result.R = Convert.ToInt32(h[..2], 16);
        result.G = Convert.ToInt32(h[2..4], 16);
        result.B = Convert.ToInt32(h[4..6], 16);
        result.Hex = $"#{h.ToUpper()}";
        RgbToHsl(result.R, result.G, result.B, out double hue, out double sat, out double light);
        result.H = hue;
        result.S = sat;
        result.L = light;
        result.Success = true;
        return result;
    }

    /// <summary>
    /// Parse RGB values to all formats (AI Accept)
    /// </summary>
    public ColorConvertResult ParseRgb(int r, int g, int b)
    {
        var result = new ColorConvertResult();

        if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
        {
            result.ErrorMessage = "RGB values must be 0-255";
            return result;
        }

        result.R = r;
        result.G = g;
        result.B = b;
        result.Hex = $"#{r:X2}{g:X2}{b:X2}";
        RgbToHsl(r, g, b, out double h, out double s, out double l);
        result.H = h;
        result.S = s;
        result.L = l;
        result.Success = true;
        return result;
    }

    /// <summary>
    /// Parse HSL values to all formats (AI Accept)
    /// </summary>
    public ColorConvertResult ParseHsl(double h, double s, double l)
    {
        var result = new ColorConvertResult();

        if (h < 0 || h > 360 || s < 0 || s > 100 || l < 0 || l > 100)
        {
            result.ErrorMessage = "H: 0-360, S: 0-100, L: 0-100";
            return result;
        }

        result.H = h;
        result.S = s;
        result.L = l;
        HslToRgb(h, s / 100, l / 100, out int r, out int g, out int b);
        result.R = r;
        result.G = g;
        result.B = b;
        result.Hex = $"#{r:X2}{g:X2}{b:X2}";
        result.Success = true;
        return result;
    }

    /// <summary>
    /// Compute comprehensive character statistics (AI Accept)
    /// </summary>
    public CharStatsResult GetCharStats(string text)
    {
        var result = new CharStatsResult();

        if (string.IsNullOrEmpty(text))
            return result;

        result.TotalChars = text.Length;
        result.TotalCharsNoSpaces = text.Count(c => !char.IsWhiteSpace(c));
        result.LineCount = text.Split('\n').Length;
        result.LetterCount = text.Count(char.IsLetter);
        result.DigitCount = text.Count(char.IsDigit);
        result.SpaceCount = text.Count(c => c == ' ');
        result.PunctuationCount = text.Count(char.IsPunctuation);
        result.ByteCountUtf8 = Encoding.UTF8.GetByteCount(text);
        result.ByteCountAscii = text.Length; // ASCII is 1 byte per char conceptually

        // AI Accept [Added]: Count Chinese characters (CJK Unified Ideographs range)
        result.ChineseCount = text.Count(c => c >= '\u4e00' && c <= '\u9fff');

        // AI Accept [Added]: Word count - split by whitespace, filter empty
        result.WordCount = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

        // AI Accept [Added]: Character frequency (top 30, skip whitespace)
        var freq = text.Where(c => !char.IsWhiteSpace(c))
            .GroupBy(c => c)
            .OrderByDescending(g => g.Count())
            .Take(30)
            .Select(g => new CharFrequency
            {
                Char = g.Key switch
                {
                    '\t' => "\\t",
                    '\r' => "\\r",
                    '\n' => "\\n",
                    _ => g.Key.ToString()
                },
                Count = g.Count(),
                Percent = Math.Round(g.Count() * 100.0 / result.TotalCharsNoSpaces, 1)
            })
            .ToList();

        result.TopFrequencies = freq;
        return result;
    }

    #region Color Helpers

    /// <summary>
    /// Convert RGB to HSL (AI Accept)
    /// </summary>
    private static void RgbToHsl(int r, int g, int b, out double h, out double s, out double l)
    {
        double rd = r / 255.0, gd = g / 255.0, bd = b / 255.0;
        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double diff = max - min;

        l = (max + min) / 2.0;

        if (Math.Abs(diff) < 0.00001)
        {
            h = 0;
            s = 0;
        }
        else
        {
            s = l > 0.5 ? diff / (2.0 - max - min) : diff / (max + min);

            if (Math.Abs(max - rd) < 0.00001)
                h = ((gd - bd) / diff + (gd < bd ? 6 : 0)) * 60;
            else if (Math.Abs(max - gd) < 0.00001)
                h = ((bd - rd) / diff + 2) * 60;
            else
                h = ((rd - gd) / diff + 4) * 60;
        }

        h = Math.Round(h, 1);
        s = Math.Round(s * 100, 1);
        l = Math.Round(l * 100, 1);
    }

    /// <summary>
    /// Convert HSL to RGB (AI Accept)
    /// </summary>
    private static void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
    {
        if (Math.Abs(s) < 0.00001)
        {
            r = g = b = (int)Math.Round(l * 255);
            return;
        }

        double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        double p = 2 * l - q;
        double hNorm = h / 360.0;

        r = (int)Math.Round(HueToRgb(p, q, hNorm + 1.0 / 3) * 255);
        g = (int)Math.Round(HueToRgb(p, q, hNorm) * 255);
        b = (int)Math.Round(HueToRgb(p, q, hNorm - 1.0 / 3) * 255);

        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2) return q;
        if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
        return p;
    }

    #endregion
}
