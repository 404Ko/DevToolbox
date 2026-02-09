using System.Security.Cryptography;
using System.Text;

namespace DevToolbox.Web.Services;

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
