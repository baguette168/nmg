using NMG.Templates;
using NMG.Shells;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace NMG.Tests;

public class RouteTemplateTests
{
    [Fact]
    public void RouteTemplate_Renders_WithoutPlaceholders()
    {
        string source = RouteTemplate.Source
            .Replace("{{NAMESPACE}}", "Test.NS")
            .Replace("{{CLASSNAME}}", "RouteCls42")
            .Replace("{{MEMBERS}}", "public string password = \"test\";")
            .Replace("{{TRIGGER_BLOCK}}", "// trigger")
            .Replace("{{RANDOM_STRING}}", "// random");
        Assert.DoesNotContain("{{", source);
    }

    [Fact]
    public void RouteTemplate_ParsesAsValidCSharp()
    {
        string source = RouteTemplate.Source
            .Replace("{{NAMESPACE}}", "Test.NS")
            .Replace("{{CLASSNAME}}", "RouteCls42")
            .Replace("{{MEMBERS}}", "public string password = \"test\";")
            .Replace("{{TRIGGER_BLOCK}}", "System.Diagnostics.Debug.WriteLine(\"ok\");")
            .Replace("{{RANDOM_STRING}}", "public static string RS(int n) { return n.ToString(); }");
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp2));
        var errors = tree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void RouteTemplate_InsertsAtPositionZero()
    {
        Assert.Contains("Insert(0,", RouteTemplate.Source);
    }
}

public class HttpListenerTemplateTests
{
    [Fact]
    public void HttpListenerTemplate_Renders_WithoutPlaceholders()
    {
        string source = HttpListenerTemplate.Source
            .Replace("{{NAMESPACE}}", "Test.NS")
            .Replace("{{CLASSNAME}}", "HLCls42")
            .Replace("{{MEMBERS}}", "public string password = \"test\";")
            .Replace("{{TRIGGER_BLOCK}}", "// trigger")
            .Replace("{{RANDOM_STRING}}", "// random")
            .Replace("{{LISTENER_URL}}", "http://*:8080/cmd/");
        Assert.DoesNotContain("{{", source);
    }

    [Fact]
    public void HttpListenerTemplate_NoVarKeyword()
    {
        // C# 2.0 compatibility: no implicit typing
        Assert.DoesNotContain("var ", HttpListenerTemplate.Source);
    }

    [Fact]
    public void HttpListenerTemplate_CreatesThread()
    {
        Assert.Contains("new Thread", HttpListenerTemplate.Source);
        Assert.Contains("HttpListener", HttpListenerTemplate.Source);
    }
}

public class BehinderTemplateTests
{
    [Fact]
    public void BehinderTrigger_UsesRawInputStream()
    {
        Assert.Contains("InputStream", BehinderNet.TriggerBlock);
        Assert.DoesNotContain("www-form", BehinderNet.TriggerBlock);
    }

    [Fact]
    public void BehinderTrigger_UsesMD5KeyBytes()
    {
        Assert.Contains("MD5CryptoServiceProvider", BehinderNet.TriggerBlock);
        Assert.Contains("Array.Copy", BehinderNet.TriggerBlock);
    }

    [Fact]
    public void BehinderMembers_NoKeyField()
    {
        // Behinder derives key from password alone, no separate key field
        Assert.DoesNotContain("public string key", BehinderNet.MembersBlock);
    }
}
