using NMG.Formats;
using Xunit;

namespace NMG.Tests;

public class FormatTests
{
    private readonly byte[] _fakeDll = Enumerable.Range(0, 6144)
        .Select(i => (byte)(i % 256)).ToArray();

    [Fact]
    public void AspxInjector_StartsWithPageDirective()
    {
        string result = AspxInjector.Wrap(_fakeDll, "Ns.Test", "Class42");
        Assert.StartsWith("<%@ Page Language=\"C#\"", result);
    }

    [Fact]
    public void AspxInjector_ContainsBase64OfDll()
    {
        string result = AspxInjector.Wrap(_fakeDll, "Ns.Test", "Class42");
        string expectedB64 = Convert.ToBase64String(_fakeDll);
        // The base64 is chunked, so check for a prefix
        string prefix = expectedB64[..40];
        Assert.Contains(prefix, result);
    }

    [Fact]
    public void AspxInjector_ContainsNamespaceAndClass()
    {
        string result = AspxInjector.Wrap(_fakeDll, "Sys.Web.Cache", "CacheProvider99");
        Assert.Contains("Sys.Web.Cache", result);
        Assert.Contains("CacheProvider99", result);
    }

    [Fact]
    public void AspxInjector_ContainsAssemblyLoad()
    {
        string result = AspxInjector.Wrap(_fakeDll, "N", "C");
        Assert.Contains("Assembly.Load", result);
        Assert.Contains("FromBase64String", result);
    }

    [Fact]
    public void AspxInjector_ContainsRunClassConstructor()
    {
        string result = AspxInjector.Wrap(_fakeDll, "N", "C");
        Assert.Contains("RunClassConstructor", result);
    }

    [Fact]
    public void AspxInjector_HasTryCatch()
    {
        string result = AspxInjector.Wrap(_fakeDll, "N", "C");
        Assert.Contains("try", result);
        Assert.Contains("catch", result);
    }

    [Fact]
    public void Base64Output_RoundTrips()
    {
        string b64 = Base64Output.Wrap(_fakeDll);
        byte[] decoded = Convert.FromBase64String(b64);
        Assert.Equal(_fakeDll, decoded);
    }

    [Fact]
    public void Base64Output_EmptyInput_ProducesEmptyString()
    {
        string b64 = Base64Output.Wrap(Array.Empty<byte>());
        Assert.Equal("", b64);
    }

    [Fact]
    public void Base64Output_LargeInput_Works()
    {
        var large = new byte[100_000];
        new Random(42).NextBytes(large);
        string b64 = Base64Output.Wrap(large);
        Assert.Equal(large, Convert.FromBase64String(b64));
    }
}
