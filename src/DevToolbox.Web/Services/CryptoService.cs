using System.Security.Cryptography;
using System.Text;

namespace DevToolbox.Web.Services;

/// <summary>
/// Crypto service interface (AI Accept)
/// </summary>
public interface ICryptoService
{
    string Md5Hash(string input);
    string Sha1Hash(string input);
    string Sha256Hash(string input);
    string Sha512Hash(string input);
    string AesEncrypt(string plainText, string key, string iv);
    string AesDecrypt(string cipherText, string key, string iv);

    /// <summary>
    /// AES encrypt with full options (AI Accept)
    /// </summary>
    string AesEncrypt(string plainText, string key, string iv, int keySize, string mode, string outputFormat);

    /// <summary>
    /// AES decrypt with full options (AI Accept)
    /// </summary>
    string AesDecrypt(string cipherText, string key, string iv, int keySize, string mode, string inputFormat);

    /// <summary>
    /// Generate random AES key as hex string (AI Accept)
    /// </summary>
    string GenerateRandomKey(int keySize);

    /// <summary>
    /// Generate random AES IV as hex string (AI Accept)
    /// </summary>
    string GenerateRandomIv();
}

/// <summary>
/// Crypto service implementation (AI Accept)
/// </summary>
public class CryptoService : ICryptoService
{
    /// <summary>
    /// MD5 hash - pure managed implementation for WASM compatibility (AI Accept [Fixed])
    /// </summary>
    public string Md5Hash(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var bytes = ManagedMd5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// SHA1 hash (AI Accept [Fixed]: use instance method for WASM compatibility)
    /// </summary>
    public string Sha1Hash(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        using var hasher = SHA1.Create();
        var bytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// SHA256 hash (AI Accept [Fixed]: use instance method for WASM compatibility)
    /// </summary>
    public string Sha256Hash(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        using var hasher = SHA256.Create();
        var bytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// SHA512 hash (AI Accept [Fixed]: use instance method for WASM compatibility)
    /// </summary>
    public string Sha512Hash(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        using var hasher = SHA512.Create();
        var bytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// AES encrypt with default settings: AES-256, CBC, Base64 output (AI Accept)
    /// </summary>
    public string AesEncrypt(string plainText, string key, string iv)
    {
        return AesEncrypt(plainText, key, iv, 256, "CBC", "Base64");
    }

    /// <summary>
    /// AES decrypt with default settings: AES-256, CBC, Base64 input (AI Accept)
    /// </summary>
    public string AesDecrypt(string cipherText, string key, string iv)
    {
        return AesDecrypt(cipherText, key, iv, 256, "CBC", "Base64");
    }

    /// <summary>
    /// AES encrypt with full options (AI Accept)
    /// </summary>
    /// <param name="plainText">Plain text to encrypt</param>
    /// <param name="key">Key as UTF-8 string or hex string</param>
    /// <param name="iv">IV as UTF-8 string or hex string</param>
    /// <param name="keySize">Key size: 128, 192, or 256</param>
    /// <param name="mode">Cipher mode: CBC or ECB</param>
    /// <param name="outputFormat">Output format: Base64 or Hex</param>
    public string AesEncrypt(string plainText, string key, string iv, int keySize, string mode, string outputFormat)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        // AI Accept [Added]: Validate key size
        var keySizeBytes = keySize / 8;
        if (keySize is not (128 or 192 or 256))
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

        using var aes = Aes.Create();
        aes.KeySize = keySize;
        aes.Key = ResolveKeyBytes(key, keySizeBytes);
        aes.Padding = PaddingMode.PKCS7;

        // AI Accept [Added]: Support CBC and ECB modes
        aes.Mode = mode.ToUpperInvariant() switch
        {
            "CBC" => CipherMode.CBC,
            "ECB" => CipherMode.ECB,
            _ => throw new ArgumentException($"Unsupported mode: {mode}")
        };

        if (aes.Mode != CipherMode.ECB)
        {
            aes.IV = ResolveKeyBytes(iv, 16);
        }

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // AI Accept [Added]: Support Base64 and Hex output
        return outputFormat.ToUpperInvariant() switch
        {
            "HEX" => Convert.ToHexString(encryptedBytes).ToLower(),
            _ => Convert.ToBase64String(encryptedBytes)
        };
    }

    /// <summary>
    /// AES decrypt with full options (AI Accept)
    /// </summary>
    /// <param name="cipherText">Cipher text to decrypt</param>
    /// <param name="key">Key as UTF-8 string or hex string</param>
    /// <param name="iv">IV as UTF-8 string or hex string</param>
    /// <param name="keySize">Key size: 128, 192, or 256</param>
    /// <param name="mode">Cipher mode: CBC or ECB</param>
    /// <param name="inputFormat">Input format: Base64 or Hex</param>
    public string AesDecrypt(string cipherText, string key, string iv, int keySize, string mode, string inputFormat)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var keySizeBytes = keySize / 8;
        if (keySize is not (128 or 192 or 256))
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

        // AI Accept [Added]: Parse cipher text from Base64 or Hex
        byte[] cipherBytes = inputFormat.ToUpperInvariant() switch
        {
            "HEX" => Convert.FromHexString(cipherText.Replace(" ", "")),
            _ => Convert.FromBase64String(cipherText)
        };

        using var aes = Aes.Create();
        aes.KeySize = keySize;
        aes.Key = ResolveKeyBytes(key, keySizeBytes);
        aes.Padding = PaddingMode.PKCS7;

        aes.Mode = mode.ToUpperInvariant() switch
        {
            "CBC" => CipherMode.CBC,
            "ECB" => CipherMode.ECB,
            _ => throw new ArgumentException($"Unsupported mode: {mode}")
        };

        if (aes.Mode != CipherMode.ECB)
        {
            aes.IV = ResolveKeyBytes(iv, 16);
        }

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// Generate random AES key as hex string (AI Accept)
    /// </summary>
    /// <param name="keySize">Key size in bits: 128, 192, or 256</param>
    public string GenerateRandomKey(int keySize)
    {
        var bytes = new byte[keySize / 8];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// Generate random AES IV (16 bytes) as hex string (AI Accept)
    /// </summary>
    public string GenerateRandomIv()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// Resolve key/iv bytes: if valid hex string of correct length, decode as hex; otherwise pad/trim as UTF-8 (AI Accept)
    /// </summary>
    private static byte[] ResolveKeyBytes(string input, int requiredLength)
    {
        input ??= string.Empty;

        // AI Accept [Added]: Try hex decode if the string looks like hex and is the right length
        if (input.Length == requiredLength * 2 && IsHexString(input))
        {
            return Convert.FromHexString(input);
        }

        // Fallback: UTF-8 encode and pad/trim
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var result = new byte[requiredLength];
        Array.Copy(inputBytes, result, Math.Min(inputBytes.Length, requiredLength));
        return result;
    }

    /// <summary>
    /// Check if a string is valid hex (AI Accept)
    /// </summary>
    private static bool IsHexString(string value)
    {
        foreach (var c in value)
        {
            if (!char.IsAsciiHexDigit(c))
                return false;
        }
        return value.Length > 0;
    }
}
