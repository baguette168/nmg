using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace NMG.Client;

public partial class MainWindow : Window
{
    private string _lastPayload = "";
    private string _lastFormat = "aspx";
    
    public MainWindow()
    {
        InitializeComponent();
        ApplyLanguage();
    }

    

    private void ApplyLanguage()
    {
        Title = L.Get("app_title");
        BtnLang.Content = L.Get("lang_btn");
        LblType.Text = L.Get("type_label");
        HintType.Text = L.Get("type_hint");
        LblShell.Text = L.Get("shell_label");
        HintShell.Text = L.Get("shell_hint");
        LblPass.Text = L.Get("pass_label");
        HintPass.Text = L.Get("pass_hint");
        LblTarget.Text = L.Get("target_label");
        HintTarget.Text = L.Get("target_hint");
        LblFormat.Text = L.Get("format_label");
        HintFormat.Text = L.Get("format_hint");
        LblSeed.Text = L.Get("seed_label");
        HintSeed.Text = L.Get("seed_hint");
        BtnGenerate.Content = L.Get("generate");
        BtnCopy.Content = L.Get("copy");
        BtnSave.Content = L.Get("save");
        TxtStatus.Text = L.Get("status_ready");
        TxtStatus.Foreground = FindResource("TextFillColorSecondaryBrush") as Brush ?? Brushes.Gray;
    }

    private void BtnLang_Click(object sender, RoutedEventArgs e)
    {
        L.Toggle();
        ApplyLanguage();
    }

    private void CmbType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (CmbShell == null || CmbType == null) return;
        var type = GetTag(CmbType);
        var shell = GetTag(CmbShell);
        if (type == "httplistener" && shell != "godzilla")
        {
            CmbShell.SelectedIndex = 0;
            MessageBox.Show(L.Get("http_listener_warn"), "NMG", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
    {
        var pass = TxtPass.Text.Trim();
        if (string.IsNullOrEmpty(pass))
        {
            SetStatus(L.Get("need_pass"), "SystemFillColorCriticalBrush");
            return;
        }

        BtnGenerate.IsEnabled = false;
        SetStatus(L.Get("generating"), "TextFillColorSecondaryBrush");

        try
        {
            var type = GetTag(CmbType);
            var shell = GetTag(CmbShell);
            var target = GetTag(CmbTarget);
            var format = GetTag(CmbFormat);
            _lastFormat = format;

            var seedText = TxtSeed.Text.Trim();
            int? seed = int.TryParse(seedText, out var s) ? s : null;

            var nmgDll = FindNmgDll();
            var tempFile = Path.Combine(Path.GetTempPath(), $"nmg_{Guid.NewGuid():N}.{format}");

            var args = $"\"{nmgDll}\" generate --type {type} --shell {shell} --pass \"{pass}\" --target {target} --format {format} -o \"{tempFile}\"";
            if (seed.HasValue) args += $" --seed {seed.Value}";

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
            if (proc == null) throw new Exception("Failed to start nmg");

            var stdout = await proc.StandardOutput.ReadToEndAsync();
            var stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
            {
                SetStatus(L.Get("err_prefix") + stderr.Trim(), "SystemFillColorCriticalBrush");
                return;
            }

            if (File.Exists(tempFile))
            {
                _lastPayload = File.ReadAllText(tempFile);
                File.Delete(tempFile);
            }

            TxtOutput.Text = _lastPayload;
            var size = System.Text.Encoding.UTF8.GetByteCount(_lastPayload);
            TxtMeta.Text = $"{type} · {shell} · {target} · {size:N0} bytes";

            SetStatus(L.Get("success"), "SystemFillColorSuccessBrush");
        }
        catch (Exception ex)
        {
            SetStatus(L.Get("err_prefix") + ex.Message, "SystemFillColorCriticalBrush");
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
        SetStatus(L.Get("copied"), "SystemFillColorSuccessBrush");
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_lastPayload)) return;
        var ext = _lastFormat == "b64" ? ".b64" : ".aspx";
        var dialog = new SaveFileDialog { Filter = $"Payload|*{ext}|All|*.*", FileName = $"nmg_payload{ext}" };
        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, _lastPayload);
            SetStatus(L.Get("saved") + dialog.FileName, "SystemFillColorSuccessBrush");
        }
    }

    private void SetStatus(string text, string brushKey)
    {
        TxtStatus.Text = text;
        TxtStatus.Foreground = FindResource(brushKey) as Brush ?? new SolidColorBrush(Color.FromRgb(0xA0,0xA0,0xA0));
    }

    private string GetTag(ComboBox cmb)
    {
        return cmb.SelectedItem is ComboBoxItem item && item.Tag != null ? item.Tag.ToString() : "";
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
