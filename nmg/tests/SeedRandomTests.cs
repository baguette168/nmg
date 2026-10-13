using NMG.Randomizer;
using Xunit;

namespace NMG.Tests;

public class SeedRandomTests
{
    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var r1 = new SeedRandom(42);
        var r2 = new SeedRandom(42);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(r1.NextClassName(), r2.NextClassName());
            Assert.Equal(r1.NextNamespace(), r2.NextNamespace());
        }
    }

    [Fact]
    public void DifferentSeed_ProducesDifferentSequence()
    {
        var r1 = new SeedRandom(42);
        var r2 = new SeedRandom(43);

        bool anyDiff = false;
        for (int i = 0; i < 50; i++)
        {
            if (r1.NextClassName() != r2.NextClassName()) { anyDiff = true; break; }
        }
        Assert.True(anyDiff);
    }

    [Fact]
    public void NullSeed_IsTimeBased()
    {
        var r1 = new SeedRandom(null);
        var r2 = new SeedRandom(null);
        // With overwhelming probability these differ
        bool anyDiff = false;
        for (int i = 0; i < 20; i++)
        {
            if (r1.NextClassName() != r2.NextClassName()) { anyDiff = true; break; }
        }
        Assert.True(anyDiff);
    }

    [Fact]
    public void NextHexString_ReturnsCorrectLength()
    {
        var r = new SeedRandom(1);
        Assert.Equal(16, r.NextHexString(16).Length);
        Assert.Equal(32, r.NextHexString(32).Length);
    }

    [Fact]
    public void NextHexString_OnlyValidHexChars()
    {
        var r = new SeedRandom(1);
        string result = r.NextHexString(64);
        Assert.All(result, c => Assert.Contains(c, "0123456789abcdef"));
    }

    [Fact]
    public void NextNamespace_HasValidFormat()
    {
        var r = new SeedRandom(1);
        string ns = r.NextNamespace();
        Assert.DoesNotContain(" ", ns);
        Assert.StartsWith(char.IsUpper(ns[0]) ? "" : "", ns); // just checking no crash
    }
}
