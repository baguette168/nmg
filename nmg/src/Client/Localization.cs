using System;
using System.Collections.Generic;
using System.Windows;

namespace NMG.Client;

/// <summary>
/// Bilingual localization manager. Switches between Chinese and English at runtime.
/// </summary>
public static class L
{
    public static bool IsChinese { get; private set; } = true;
    public static event Action LanguageChanged;

    public static void Toggle()
    {
        IsChinese = !IsChinese;
        LanguageChanged?.Invoke();
    }

    static readonly Dictionary<string, string[]> S = new()
    {
        ["app_title"] = new[] { "NMG — .NET 内存马生成器", "NMG — .NET Memory-shell Generator" },
        ["app_sub"] = new[] { "参数化内存马生成工具", "Parameterized memory shell generator" },
        ["lang_btn"] = new[] { "EN", "中文" },
        ["type_label"] = new[] { "内存马类型", "MEMORY SHELL TYPE" },
        ["type_hint"] = new[] { "ASP.NET 管道中的注入点", "Target injection point in the ASP.NET pipeline" },
        ["shell_label"] = new[] { "协议", "SHELL PROTOCOL" },
        ["shell_hint"] = new[] { "Webshell 管理工具通信协议", "Webshell manager communication protocol" },
        ["pass_label"] = new[] { "连接密码", "CONNECTION PASSWORD" },
        ["pass_hint"] = new[] { "Webshell 客户端认证用", "Used by the webshell client to authenticate" },
        ["target_label"] = new[] { "目标框架", "TARGET FRAMEWORK" },
        ["target_hint"] = new[] { "Payload 编译目标", "Payload compilation target" },
        ["format_label"] = new[] { "输出格式", "OUTPUT FORMAT" },
        ["format_hint"] = new[] { "Payload 投递方式", "How the payload is delivered" },
        ["seed_label"] = new[] { "随机种子", "RANDOMIZATION SEED" },
        ["seed_hint"] = new[] { "固定种子 = 确定性输出", "Fixed seed = deterministic output" },
        ["generate"] = new[] { "生成 Payload", "Generate Payload" },
        ["generating"] = new[] { "生成中...", "Generating..." },
        ["copy"] = new[] { "复制", "Copy" },
        ["save"] = new[] { "另存为...", "Save As..." },
        ["saved"] = new[] { "已保存到 ", "Saved to " },
        ["copied"] = new[] { "已复制到剪贴板", "Copied to clipboard" },
        ["success"] = new[] { "Payload 生成成功", "Payload generated successfully" },
        ["need_pass"] = new[] { "错误：密码不能为空", "Error: password is required" },
        ["status_ready"] = new[] { "配置参数后点击生成", "Configure parameters and click Generate" },
        ["err_prefix"] = new[] { "错误：", "Error: " },
        ["http_listener_warn"] = new[] { "HttpListener 目前仅支持 Godzilla 协议", "HttpListener only supports Godzilla protocol" },
        ["output_placeholder"] = new[] { "", "" },
    };

    public static string Get(string key)
    {
        if (S.TryGetValue(key, out var pair))
            return IsChinese ? pair[0] : pair[1];
        return key;
    }
}
