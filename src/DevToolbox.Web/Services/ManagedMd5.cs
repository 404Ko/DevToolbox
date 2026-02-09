using System.Buffers.Binary;

namespace DevToolbox.Web.Services;

/// <summary>
/// Pure managed MD5 implementation (RFC 1321) for Blazor WASM compatibility.
/// Browser SubtleCrypto does NOT support MD5, so we need a software fallback. (AI Accept)
/// </summary>
internal static class ManagedMd5
{
    // AI Accept [Added]: MD5 per-round shift amounts
    private static readonly int[] S =
    {
        7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22,
        5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20,
        4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23,
        6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21,
    };

    // AI Accept [Added]: MD5 sine-derived constants (floor(2^32 * abs(sin(i+1))))
    private static readonly uint[] K =
    {
        0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
        0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
        0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
        0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
        0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
        0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
        0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
        0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
        0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
        0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
        0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
        0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
        0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
        0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
        0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
        0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391,
    };

    /// <summary>
    /// Compute MD5 hash of input bytes, returns 16-byte digest (AI Accept)
    /// </summary>
    public static byte[] ComputeHash(byte[] message)
    {
        // AI Accept [Added]: Step 1 - Pad message
        var padded = PadMessage(message);

        // Step 2 - Initialize state
        uint a0 = 0x67452301;
        uint b0 = 0xefcdab89;
        uint c0 = 0x98badcfe;
        uint d0 = 0x10325476;

        // Step 3 - Process each 64-byte (512-bit) block
        for (var offset = 0; offset < padded.Length; offset += 64)
        {
            var m = new uint[16];
            for (var j = 0; j < 16; j++)
            {
                m[j] = BinaryPrimitives.ReadUInt32LittleEndian(
                    padded.AsSpan(offset + j * 4, 4));
            }

            uint a = a0, b = b0, c = c0, d = d0;

            for (var i = 0; i < 64; i++)
            {
                uint f;
                int g;

                if (i < 16)
                {
                    f = (b & c) | (~b & d);
                    g = i;
                }
                else if (i < 32)
                {
                    f = (d & b) | (~d & c);
                    g = (5 * i + 1) % 16;
                }
                else if (i < 48)
                {
                    f = b ^ c ^ d;
                    g = (3 * i + 5) % 16;
                }
                else
                {
                    f = c ^ (b | ~d);
                    g = (7 * i) % 16;
                }

                var temp = d;
                d = c;
                c = b;
                b = b + RotateLeft(a + f + K[i] + m[g], S[i]);
                a = temp;
            }

            a0 += a;
            b0 += b;
            c0 += c;
            d0 += d;
        }

        // Step 4 - Output digest (little-endian)
        var digest = new byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(digest.AsSpan(0), a0);
        BinaryPrimitives.WriteUInt32LittleEndian(digest.AsSpan(4), b0);
        BinaryPrimitives.WriteUInt32LittleEndian(digest.AsSpan(8), c0);
        BinaryPrimitives.WriteUInt32LittleEndian(digest.AsSpan(12), d0);

        return digest;
    }

    /// <summary>
    /// MD5 message padding: append 1-bit, zeros, then 64-bit length (AI Accept)
    /// </summary>
    private static byte[] PadMessage(byte[] message)
    {
        var originalLength = message.Length;
        var bitLength = (ulong)originalLength * 8;

        // Pad to 56 mod 64 bytes, then append 8-byte length
        var paddingLength = (56 - (originalLength + 1) % 64 + 64) % 64 + 1;
        var padded = new byte[originalLength + paddingLength + 8];

        Array.Copy(message, padded, originalLength);
        padded[originalLength] = 0x80;

        BinaryPrimitives.WriteUInt64LittleEndian(
            padded.AsSpan(padded.Length - 8), bitLength);

        return padded;
    }

    private static uint RotateLeft(uint x, int n) => (x << n) | (x >> (32 - n));
}
