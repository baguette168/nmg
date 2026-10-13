using NMG.Templates;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace NMG.Tests;

public class TemplateTests
{
    [Fact]
    public void VppTemplate_Renders_WithoutPlaceholdersRemaining()
    {
        string source = VppTemplate.Source
            .Replace("{{NAMESPACE}}", "Sys.Web.Cache")
            .Replace("{{CLASSNAME}}", "CacheProvider42")
            .Replace("{{MEMBERS}}", "public string password = \"test\";")
            .Replace("{{RANDOM_STRING}}", "// random string block")
            .Replace("{{TRIGGER_BLOCK}}", "// trigger");

        Assert.DoesNotContain("{{", source);
        Assert.DoesNotContain("}}", source);
    }

    [Fact]
    public void VppTemplate_Renders_ValidCSharp()
    {
        string source = VppTemplate.Source
            .Replace("{{NAMESPACE}}", "Sys.Web.Cache")
            .Replace("{{CLASSNAME}}", "CacheProvider42")
            .Replace("{{MEMBERS}}", "public string password = \"test\";\npublic string key = \"abc123\";")
            .Replace("{{RANDOM_STRING}}", "public static string RS(int n) { return n.ToString(); }")
            .Replace("{{TRIGGER_BLOCK}}", "System.Diagnostics.Debug.WriteLine(\"ok\");");

        var tree = CSharpSyntaxTree.ParseText(source,
            new CSharpParseOptions(LanguageVersion.CSharp2));
        var diags = tree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToList();
        Assert.True(diags.Count == 0,
            "Template should parse without errors: " + string.Join("\n", diags.Select(d => d.GetMessage())));
    }

    [Fact]
    public void VppTemplate_ContainsRegistrationCall()
    {
        Assert.Contains("RegisterVirtualPathProviderInternal", VppTemplate.Source);
        Assert.Contains("try", VppTemplate.Source);
        Assert.Contains("catch", VppTemplate.Source);
    }

    [Fact]
    public void VppTemplate_TryCatchWrapsRegistration()
    {
        // NFR-1: All execution paths wrapped in try-catch
        int staticCtorStart = VppTemplate.Source.IndexOf("static {{CLASSNAME}}()");
        int firstCatchAfter = VppTemplate.Source.IndexOf("catch", staticCtorStart);
        Assert.True(staticCtorStart >= 0, "Static constructor must exist");
        Assert.True(firstCatchAfter > staticCtorStart, "try-catch must wrap static ctor");
    }
}
