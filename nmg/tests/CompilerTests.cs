using NMG.Compiler;
using NMG.Templates;
using NMG.Shells;
using Xunit;

namespace NMG.Tests;

public class CompilerTests
{
    private string RenderVppSource(string pass = "test", string key = "0123456789abcdef")
    {
        byte xorKey = 0x2A;
        string members = GodzillaCSharp.MembersBlock
            .Replace("{{PASSWORD_ENC}}", Randomizer.StringObfuscator.Encode(pass, xorKey))
            .Replace("{{KEY_ENC}}", Randomizer.StringObfuscator.Encode(key, xorKey))
            .Replace("{{XOR_KEY}}", "0x2A");
        string trigger = GodzillaCSharp.TriggerBlock
            .Replace("{{PASSWORD}}", "\"" + pass + "\"")
            .Replace("{{KEY}}", "\"" + key + "\"");
        string randomStr = GodzillaCSharp.RandomStringBlock;

        return VppTemplate.Source
            .Replace("{{NAMESPACE}}", "Test.NS")
            .Replace("{{CLASSNAME}}", "TestClass42")
            .Replace("{{MEMBERS}}", members)
            .Replace("{{TRIGGER_BLOCK}}", trigger)
            .Replace("{{RANDOM_STRING}}", randomStr);
    }

    [Fact]
    public void Compile_Net40_ProducesValidPE()
    {
        var compiler = new RoslynCompiler();
        string source = RenderVppSource();
        var (success, dll, diags) = compiler.Compile(source, "net40");
        Assert.True(success, string.Join("\n", diags));
        Assert.True(dll.Length > 0);
        Assert.Equal(0x4D, dll[0]);
        Assert.Equal(0x5A, dll[1]);
    }

    [Fact]
    public void Compile_Net472_ProducesValidPE()
    {
        var compiler = new RoslynCompiler();
        string source = RenderVppSource();
        var (success, dll, diags) = compiler.Compile(source, "net472");
        Assert.True(success, string.Join("\n", diags));
        Assert.True(dll.Length > 0);
        Assert.Equal(0x4D, dll[0]);
    }

    [Fact]
    public void Compile_BadSource_ReturnsDiagnostics()
    {
        var compiler = new RoslynCompiler();
        var (success, dll, diags) = compiler.Compile("this is not valid C#", "net40");
        Assert.False(success);
        Assert.Empty(dll);
        Assert.NotEmpty(diags);
    }

    [Fact]
    public void Compile_SameSource_ProducesSameBytes()
    {
        var compiler = new RoslynCompiler();
        string source = RenderVppSource("dettest", "abcdef0123456789");
        var (_, dll1, _) = compiler.Compile(source, "net40");
        var (_, dll2, _) = compiler.Compile(source, "net40");
        Assert.Equal(dll1, dll2);
    }

    [Fact]
    public void Compile_DifferentSource_ProducesDifferentBytes()
    {
        var compiler = new RoslynCompiler();
        string s1 = RenderVppSource("pass1", "key1111111111111");
        string s2 = RenderVppSource("pass2", "key2222222222222");
        var (_, dll1, _) = compiler.Compile(s1, "net40");
        var (_, dll2, _) = compiler.Compile(s2, "net40");
        Assert.NotEqual(dll1, dll2);
    }

    [Fact]
    public void Compile_UnknownTarget_Throws()
    {
        var compiler = new RoslynCompiler();
        string source = RenderVppSource();
        var (success, _, diags) = compiler.Compile(source, "net999");
        Assert.False(success);
    }

    [Fact]
    public void Compile_VppTemplate_ProducesNonEmptyDll()
    {
        var compiler = new RoslynCompiler();
        string source = RenderVppSource();
        var (success, dll, diags) = compiler.Compile(source, "net40");
        Assert.True(success, string.Join("\n", diags));
        Assert.True(dll.Length > 1000, "VPP DLL should be > 1KB");
    }
}
