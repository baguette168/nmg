using System;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace NMG.Client;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WebView.CoreWebView2InitializationCompleted += WebView_Initialized;
        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        var env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Path.GetTempPath(), "NMG_WebView2"));
        await WebView.EnsureCoreWebView2Async(env);
        WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        WebView.CoreWebView2.WebMessageReceived += OnWebMessage;
        
        var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", "index.html");
        WebView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
    }

    private void WebView_Initialized(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        // Add the bridge object for JS interop
        WebView.CoreWebView2.AddHostObjectToScript("bridge", new Bridge());
    }

    private void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        // Handle messages from JS if needed
    }
}

/// <summary>
/// Bridge class exposed to JavaScript for calling nmg.exe
/// </summary>
[System.Runtime.InteropServices.ComVisible(true)]
public class Bridge
{
    public BridgeResult Generate(string type, string shell, string pass, string target, string format, int seed)
    {
        try
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"nmg_{Guid.NewGuid():N}.{format}");
            var args = $"\"{FindNmgDll()}\" generate --type {type} --shell {shell} --pass \"{pass}\" --target {target} --format {format} -o \"{tempFile}\"";
            if (seed > 0) args += $" --seed {seed}";

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = System.Diagnostics.Process.Start(psi);
            if (proc == null) return new BridgeResult { Error = "Failed to start nmg" };

            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                return new BridgeResult { Error = stderr.Trim() };

            var payload = "";
            if (File.Exists(tempFile))
            {
                payload = File.ReadAllText(tempFile);
                File.Delete(tempFile);
            }

            // Extract connection info from stdout
            var key = Extract(stdout, @"key:\s+(\S+)");
            var passOut = pass;

            return new BridgeResult
            {
                Payload = payload,
                Connection = new ConnectionInfo { Pass = passOut, Key = key }
            };
        }
        catch (Exception ex)
        {
            return new BridgeResult { Error = ex.Message };
        }
    }

    public void SaveFile(string content, string extension)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = $"Payload|*{extension}|All|*.*",
            FileName = $"nmg_payload{extension}"
        };
        if (dialog.ShowDialog() == true)
            File.WriteAllText(dialog.FileName, content);
    }

    private string Extract(string text, string pattern)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
        return match.Success ? match.Groups[1].Value : "";
    }

    private string FindNmgDll()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "src", "bin", "Debug", "net8.0", "nmg.dll");
            if (File.Exists(candidate)) return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        var exe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nmg.exe");
        if (File.Exists(exe)) return exe;
        throw new FileNotFoundException("nmg.dll not found");
    }
}

public class BridgeResult
{
    public string? Error { get; set; }
    public string? Payload { get; set; }
    public ConnectionInfo? Connection { get; set; }
}

public class ConnectionInfo
{
    public string? Pass { get; set; }
    public string? Key { get; set; }
}
