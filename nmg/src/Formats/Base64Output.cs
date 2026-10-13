namespace NMG.Formats;

public static class Base64Output
{
    public static string Wrap(byte[] dllBytes)
    {
        return Convert.ToBase64String(dllBytes);
    }
}
