using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace NMG.Compiler;

public class RoslynCompiler
{
    private static Dictionary<string, string>? _refAssemblyCache;

    public (bool Success, byte[] Dll, string[] Diagnostics) Compile(string source, string targetFramework)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source,
            new CSharpParseOptions(LanguageVersion.CSharp2));

        var refs = GetReferences(targetFramework);
        if (refs == null || refs.Count == 0)
            return (false, Array.Empty<byte>(), new[] { "No reference assemblies found for " + targetFramework });

        var compilation = CSharpCompilation.Create(
            "NmgPayload",
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                platform: Platform.AnyCpu,
                allowUnsafe: false,
                deterministic: true));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var diags = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => $"{d.Id}: {d.GetMessage()}")
                .ToArray();
            return (false, Array.Empty<byte>(), diags);
        }

        return (true, ms.ToArray(), Array.Empty<string>());
    }

    private static List<MetadataReference>? GetReferences(string targetFramework)
    {
        var refDir = FindReferenceAssemblies(targetFramework);
        if (refDir == null) return null;

        var refs = new List<MetadataReference>();
        string[] neededDlls = targetFramework switch
        {
            "net20" => new[] { "mscorlib.dll", "System.dll", "System.Web.dll" },
            "net40" => new[] { "mscorlib.dll", "System.dll", "System.Core.dll", "System.Web.dll",
                               "System.Runtime.Remoting.dll" },
            "net472" => new[] { "mscorlib.dll", "System.dll", "System.Core.dll", "System.Web.dll",
                                "System.Runtime.Remoting.dll" },
            _ => Array.Empty<string>()
        };
        if (neededDlls.Length == 0) return null;

        foreach (var dll in neededDlls)
        {
            var path = Path.Combine(refDir, dll);
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"[compiler] missing: {path}");
                return null;
            }
            refs.Add(MetadataReference.CreateFromFile(path));
        }
        return refs;
    }

    private static string? FindReferenceAssemblies(string targetFramework)
    {
        if (_refAssemblyCache != null && _refAssemblyCache.TryGetValue(targetFramework, out var cached))
            return cached;

        string version = targetFramework switch
        {
            "net20" => "v2.0.50727",
            "net40" => "v4.0",
            "net472" => "v4.7.2",
            _ => ""
        };
        if (string.IsNullOrEmpty(version)) return null;

        var nugetDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget", "packages");

        var searchPaths = new List<string>
        {
            Path.Combine(nugetDir,
                $"microsoft.netframework.referenceassemblies.{targetFramework}", "1.0.3",
                "build", ".NETFramework", version),
            Path.Combine(AppContext.BaseDirectory, "refs", targetFramework, version),
        };

        foreach (var path in searchPaths)
        {
            var mscorlib = Path.Combine(path, "mscorlib.dll");
            if (Directory.Exists(path) && File.Exists(mscorlib))
            {
                _refAssemblyCache ??= new Dictionary<string, string>();
                _refAssemblyCache[targetFramework] = path;
                return path;
            }
        }

        Console.Error.WriteLine("[compiler] searched paths:");
        foreach (var p in searchPaths)
            Console.Error.WriteLine($"  {p} -> exists={Directory.Exists(p)}, mscorlib={File.Exists(Path.Combine(p, "mscorlib.dll"))}");
        return null;
    }
}
