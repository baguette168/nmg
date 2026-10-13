namespace NMG.Randomizer;

/// <summary>
/// XOR-based string obfuscation for payload templates.
/// Encodes sensitive strings at generation time, emits C# 2.0 decoder code.
/// Prevents static analysis from finding suspicious strings in compiled DLL.
/// </summary>
public static class StringObfuscator
{
    /// <summary>
    /// Encode a string with XOR and return as C# byte array literal.
    /// </summary>
    public static string Encode(string plaintext, byte xorKey)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var encoded = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            encoded[i] = (byte)(bytes[i] ^ xorKey);
        }
        return "new byte[] { " + string.Join(", ", encoded.Select(b => "0x" + b.ToString("X2"))) + " }";
    }

    /// <summary>
    /// Generate a C# 2.0 compatible decoder method that can be embedded in templates.
    /// </summary>
    public const string DecoderMethod = """
        internal static string XD(byte[] data, byte key)
        {
            byte[] decoded = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                decoded[i] = (byte)(data[i] ^ key);
            }
            return System.Text.Encoding.UTF8.GetString(decoded);
        }
        """;

    /// <summary>
    /// Generate a random XOR key.
    /// </summary>
    public static byte RandomKey(SeedRandom rng)
    {
        return (byte)rng.NextHexString(2).ConvertToByte();
    }
}

internal static class ByteExtensions
{
    public static int ConvertToByte(this string hex2)
    {
        return System.Convert.ToInt32(hex2, 16);
    }
}
