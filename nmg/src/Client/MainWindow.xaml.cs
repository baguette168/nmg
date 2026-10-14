using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace NMG.Client;

public partial class MainWindow : Window
{
    private string _lastPayload = "";
    private string _lastFormat = "aspx";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void CmbType_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (CmbShell == null) return;
        var type = GetTag(CmbType);
        var shell = GetTag(CmbShell);
        if (type == "httplistener" && shell != "godzilla")
        {
            CmbShell.SelectedIndex = 0; // Reset to Godzilla
            MessageBox.Show("HttpListener currently only supports Godzilla protocol.", "NMG",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
    {
        var pass = TxtPass.Text.Trim();
        if (string.IsNullOrEmpty(pass))
        {
            TxtStatus.Text = "Error: password is required";
            TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C48B8B"));
            return;
        }

        BtnGenerate.IsEnabled = false;
        TxtStatus.Text = "Generating...";
        TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9E9E9E"));

        try
        {
            var type = GetTag(CmbType);
            var shell = GetTag(CmbShell);
            var target = GetTag(CmbTarget);
            var format = GetTag(CmbFormat);
            _lastFormat = format;

            var seedText = TxtSeed.Text.Trim();
            int? seed = int.TryParse(seedText, out var s) ? s : null;

            // Build CLI args
            var args = $"generate --type {type} --shell {shell} --pass \"{pass}\" --target {target} --format {format}";
            if (seed.HasValue) args += $" --seed {seed.Value}";

            // Run nmg CLI
            var nmgDll = FindNmgDll();
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{nmgDll}\" {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = System.Diagnostics.Process.Start(psi);
            if (proc == null) throw new Exception("Failed to start nmg process");

            var stdout = await proc.StandardOutput.ReadToEndAsync();
            var stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
            {
                TxtStatus.Text = $"Error: {stderr.Trim()}";
                TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C48B8B"));
                return;
            }

            // Parse payload from output
            if (format == "b64")
            {
                var lines = stdout.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Trim().Length > 100 && !line.Trim().StartsWith("[+]"))
                    {
                        _lastPayload = line.Trim();
                        break;
                    }
                }
            }
            else
            {
                // For aspx, we need to run again with -o to a temp file
                var tempFile = Path.Combine(Path.GetTempPath(), $"nmg_{Guid.NewGuid():N}.aspx");
                var args2 = args.Replace($"--format {format}", $"--format aspx -o \"{tempFile}\"");
                psi.Arguments = $"\"{nmgDll}\" {args2}";
                var proc2 = System.Diagnostics.Process.Start(psi);
                await proc2.StandardOutput.ReadToEndAsync();
                await proc2.StandardError.ReadToEndAsync();
                await proc2.WaitForExitAsync();
                if (File.Exists(tempFile))
                {
                    _lastPayload = File.ReadAllText(tempFile);
                    File.Delete(tempFile);
                }
            }

            TxtOutput.Text = _lastPayload;
            var size = System.Text.Encoding.UTF8.GetByteCount(_lastPayload);
            TxtMeta.Text = $"{type} · {shell} · {target} · {size:N0} bytes";

            TxtStatus.Text = "Payload generated successfully";
            TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8BC4A4"));
        }
        catch (Exception ex)
        {
            TxtStatus.Text = $"Error: {ex.Message}";
            TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C48B8B"));
        }
        finally
        {
            BtnGenerate.IsEnabled = true;
        }
    }

    private void BtnCopy_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_lastPayload)) return;
        Clipboard.SetText(_lastPayload);
        TxtStatus.Text = "Copied to clipboard";
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_lastPayload)) return;
        var ext = _lastFormat == "b64" ? ".b64" : ".aspx";
        var dialog = new SaveFileDialog
        {
            Filter = $"Payload files|*{ext}|All files|*.*",
            FileName = $"nmg_payload{ext}"
        };
        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, _lastPayload);
            TxtStatus.Text = $"Saved to {dialog.FileName}";
        }
    }

    private string GetTag(System.Windows.Controls.ComboBox cmb)
    {
        if (cmb.SelectedItem is System.Windows.Controls.ComboBoxItem item && item.Tag != null)
            return item.Tag.ToString();
        return "";
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
        throw new FileNotFoundException("nmg.dll not found");
    }
}
