namespace NMG.Randomizer;

/// <summary>
/// Seeded randomization engine for deterministic payload generation.
/// Same seed produces identical output across runs (NFR-5).
/// </summary>
public class SeedRandom
{
    private readonly Random _rng;
    private const string LowerAlpha = "abcdefghijklmnopqrstuvwxyz";
    private const string UpperAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string HexLower = "0123456789abcdef";

    public SeedRandom(int? seed)
    {
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public string NextNamespace()
    {
        // e.g. "Sys.Net.Http.Reflect.Loader" style - look like framework code
        var parts = new List<string>();
        string[] candidates = {
            "Sys", "Web", "Net", "Http", "Cache", "Data", "Core",
            "Reflect", "Loader", "Provider", "Handler", "Module",
            "Framework", "Runtime", "Host", "Config"
        };
        int partCount = _rng.Next(2, 4);
        for (int i = 0; i < partCount; i++)
        {
            parts.Add(candidates[_rng.Next(candidates.Length)]);
        }
        return string.Join(".", parts);
    }

    public string NextClassName()
    {
        // e.g. "VirtualPathProviderBase", "CacheManagerInternal" style
        string[] prefixes = { "Cache", "Path", "Virtual", "Host", "Config", "Data", "Http", "Web", "Core", "App" };
        string[] suffixes = { "Provider", "Manager", "Handler", "Resolver", "Wrapper", "Helper", "Context", "Engine", "Base", "Loader" };
        return prefixes[_rng.Next(prefixes.Length)] + suffixes[_rng.Next(suffixes.Length)] + _rng.Next(10, 99);
    }

    public string NextVirtualPath()
    {
        // e.g. "/App_Data/Config/CacheProvider.axd" style - look like a resource
        string[] dirs = { "App_Data", "bin", "Config", "Data", "Resources", "Content", "Shared" };
        string[] names = { "Config", "Settings", "Cache", "Data", "Resource", "Module", "Provider", "Handler", "Index", "Main" };
        string[] exts = { ".axd", ".ashx", ".aspx", ".asmx", ".resx", ".config" };
        return "/" + dirs[_rng.Next(dirs.Length)] + "/" + names[_rng.Next(names.Length)] + _rng.Next(0, 100) + exts[_rng.Next(exts.Length)];
    }

    public string NextHexString(int length)
    {
        var sb = new System.Text.StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(HexLower[_rng.Next(HexLower.Length)]);
        }
        return sb.ToString();
    }

    public string NextRandomString(int minLength, int maxLength)
    {
        int length = _rng.Next(minLength, maxLength + 1);
        const string chars = LowerAlpha + UpperAlpha + Digits;
        var sb = new System.Text.StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[_rng.Next(chars.Length)]);
        }
        return sb.ToString();
    }
}
