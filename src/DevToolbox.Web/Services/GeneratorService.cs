using System.Security.Cryptography;
using System.Text;

namespace DevToolbox.Web.Services;

/// <summary>
/// Password strength level (AI Accept)
/// </summary>
public enum PasswordStrengthLevel
{
    VeryWeak,
    Weak,
    Medium,
    Strong,
    VeryStrong
}

/// <summary>
/// Password strength evaluation result (AI Accept)
/// </summary>
public class PasswordStrengthResult
{
    public PasswordStrengthLevel Level { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Score { get; set; }
    public double EntropyBits { get; set; }
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// Generator service interface (AI Accept)
/// </summary>
public interface IGeneratorService
{
    string GenerateGuid(string format = "D");
    List<string> GenerateMultipleGuids(int count, string format = "D");
    string GenerateRandomString(int length, bool upper = true, bool lower = true, bool numbers = true, bool special = false);
    List<string> GenerateMultipleRandomStrings(int count, int length, bool upper = true, bool lower = true, bool numbers = true, bool special = false);
    string GeneratePassword(int length = 16);

    /// <summary>
    /// Generate password with configurable options (AI Accept)
    /// </summary>
    string GeneratePassword(int length, bool upper, bool lower, bool numbers, bool special, bool excludeAmbiguous);

    /// <summary>
    /// Generate multiple passwords (AI Accept)
    /// </summary>
    List<string> GenerateMultiplePasswords(int count, int length, bool upper, bool lower, bool numbers, bool special, bool excludeAmbiguous);

    /// <summary>
    /// Evaluate password strength (AI Accept)
    /// </summary>
    PasswordStrengthResult EvaluateStrength(string password);
}

/// <summary>
/// Generator service implementation (AI Accept)
/// </summary>
public class GeneratorService : IGeneratorService
{
    private const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerChars = "abcdefghijklmnopqrstuvwxyz";
    private const string NumberChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    // AI Accept [Added]: Ambiguous characters to exclude (O/0, l/1/I, etc.)
    private const string AmbiguousChars = "O0lI1|";

    /// <summary>
    /// Generate single GUID (AI Accept)
    /// </summary>
    public string GenerateGuid(string format = "D")
    {
        return Guid.NewGuid().ToString(format);
    }

    /// <summary>
    /// Generate multiple GUIDs (AI Accept)
    /// </summary>
    public List<string> GenerateMultipleGuids(int count, string format = "D")
    {
        var result = new List<string>();
        for (int i = 0; i < count; i++)
        {
            result.Add(GenerateGuid(format));
        }
        return result;
    }

    /// <summary>
    /// Generate random string (AI Accept)
    /// </summary>
    public string GenerateRandomString(int length, bool upper = true, bool lower = true, bool numbers = true, bool special = false)
    {
        var chars = new StringBuilder();
        if (upper) chars.Append(UpperChars);
        if (lower) chars.Append(LowerChars);
        if (numbers) chars.Append(NumberChars);
        if (special) chars.Append(SpecialChars);

        if (chars.Length == 0)
            chars.Append(LowerChars); // Default

        var charArray = chars.ToString();
        var result = new StringBuilder(length);

        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        for (int i = 0; i < length; i++)
        {
            result.Append(charArray[bytes[i] % charArray.Length]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Generate multiple random strings (AI Accept)
    /// </summary>
    public List<string> GenerateMultipleRandomStrings(int count, int length, bool upper = true, bool lower = true, bool numbers = true, bool special = false)
    {
        var result = new List<string>();
        for (int i = 0; i < count; i++)
        {
            result.Add(GenerateRandomString(length, upper, lower, numbers, special));
        }
        return result;
    }

    /// <summary>
    /// Generate secure password (AI Accept)
    /// </summary>
    public string GeneratePassword(int length = 16)
    {
        // Ensure password contains at least one of each type
        var password = new StringBuilder();

        using var rng = RandomNumberGenerator.Create();

        // Add at least one of each required type
        password.Append(GetRandomChar(UpperChars, rng));
        password.Append(GetRandomChar(LowerChars, rng));
        password.Append(GetRandomChar(NumberChars, rng));
        password.Append(GetRandomChar(SpecialChars, rng));

        // Fill rest with random chars from all types
        var allChars = UpperChars + LowerChars + NumberChars + SpecialChars;
        for (int i = 4; i < length; i++)
        {
            password.Append(GetRandomChar(allChars, rng));
        }

        // Shuffle the password
        return ShuffleString(password.ToString(), rng);
    }

    /// <summary>
    /// Generate password with configurable char types and ambiguous exclusion (AI Accept)
    /// </summary>
    public string GeneratePassword(int length, bool upper, bool lower, bool numbers, bool special, bool excludeAmbiguous)
    {
        // AI Accept [Added]: Build charset based on options
        var charSets = new List<string>();
        if (upper) charSets.Add(UpperChars);
        if (lower) charSets.Add(LowerChars);
        if (numbers) charSets.Add(NumberChars);
        if (special) charSets.Add(SpecialChars);

        // Default: at least lowercase
        if (charSets.Count == 0)
            charSets.Add(LowerChars);

        // AI Accept [Added]: Remove ambiguous characters if requested
        var filteredSets = charSets.Select(s =>
            excludeAmbiguous ? new string(s.Where(c => !AmbiguousChars.Contains(c)).ToArray()) : s
        ).Where(s => s.Length > 0).ToList();

        if (filteredSets.Count == 0)
            filteredSets.Add(new string(LowerChars.Where(c => !AmbiguousChars.Contains(c)).ToArray()));

        var allChars = string.Concat(filteredSets);

        using var rng = RandomNumberGenerator.Create();
        var password = new StringBuilder(length);

        // AI Accept [Added]: Guarantee at least one char from each selected type
        foreach (var set in filteredSets)
        {
            if (password.Length < length)
                password.Append(GetRandomChar(set, rng));
        }

        // AI Accept [Added]: Fill remaining with random chars from combined pool
        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars, rng));
        }

        return ShuffleString(password.ToString(), rng);
    }

    /// <summary>
    /// Generate multiple passwords in batch (AI Accept)
    /// </summary>
    public List<string> GenerateMultiplePasswords(int count, int length, bool upper, bool lower, bool numbers, bool special, bool excludeAmbiguous)
    {
        var result = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            result.Add(GeneratePassword(length, upper, lower, numbers, special, excludeAmbiguous));
        }
        return result;
    }

    /// <summary>
    /// Evaluate password strength based on entropy and composition (AI Accept)
    /// </summary>
    public PasswordStrengthResult EvaluateStrength(string password)
    {
        var result = new PasswordStrengthResult();

        if (string.IsNullOrEmpty(password))
        {
            result.Level = PasswordStrengthLevel.VeryWeak;
            result.Label = "极弱";
            result.Score = 0;
            return result;
        }

        // AI Accept [Added]: Count character types present
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        // AI Accept [Added]: Calculate charset size for entropy
        int charsetSize = 0;
        if (hasUpper) charsetSize += 26;
        if (hasLower) charsetSize += 26;
        if (hasDigit) charsetSize += 10;
        if (hasSpecial) charsetSize += 27;
        if (charsetSize == 0) charsetSize = 26;

        // AI Accept [Added]: Entropy = length * log2(charsetSize)
        result.EntropyBits = password.Length * Math.Log2(charsetSize);

        // AI Accept [Added]: Score based on entropy
        int typeCount = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);
        int score = 0;
        score += Math.Min(password.Length * 4, 40); // Length contribution (max 40)
        score += typeCount * 10;                    // Type diversity (max 40)
        score += (int)(result.EntropyBits / 6.4);   // Entropy contribution (max ~20)
        result.Score = Math.Min(score, 100);

        // AI Accept [Added]: Determine strength level
        (result.Level, result.Label) = result.Score switch
        {
            < 20 => (PasswordStrengthLevel.VeryWeak, "极弱"),
            < 40 => (PasswordStrengthLevel.Weak, "弱"),
            < 60 => (PasswordStrengthLevel.Medium, "中等"),
            < 80 => (PasswordStrengthLevel.Strong, "强"),
            _ => (PasswordStrengthLevel.VeryStrong, "极强"),
        };

        // AI Accept [Added]: Generate improvement suggestions
        if (password.Length < 8) result.Suggestions.Add("长度至少 8 个字符");
        if (password.Length < 12) result.Suggestions.Add("建议长度 12 个字符以上");
        if (!hasUpper) result.Suggestions.Add("添加大写字母");
        if (!hasLower) result.Suggestions.Add("添加小写字母");
        if (!hasDigit) result.Suggestions.Add("添加数字");
        if (!hasSpecial) result.Suggestions.Add("添加特殊字符");

        // AI Accept [Added]: Check for repeated patterns
        if (password.Distinct().Count() < password.Length / 2)
            result.Suggestions.Add("重复字符过多，建议增加多样性");

        return result;
    }

    /// <summary>
    /// Get random char from string (AI Accept)
    /// </summary>
    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var bytes = new byte[1];
        rng.GetBytes(bytes);
        return chars[bytes[0] % chars.Length];
    }

    /// <summary>
    /// Shuffle string (AI Accept)
    /// </summary>
    private static string ShuffleString(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();
        var n = array.Length;

        while (n > 1)
        {
            var bytes = new byte[1];
            rng.GetBytes(bytes);
            var k = bytes[0] % n;
            n--;
            (array[n], array[k]) = (array[k], array[n]);
        }

        return new string(array);
    }
}
