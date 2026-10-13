<%@ Page Language="C#" %>
<%
var info = new System.Text.StringBuilder();
info.Append("{");
info.Append("\"clr\":\"" + System.Environment.Version + "\"");
info.Append(",\"os\":\"" + System.Environment.OSVersion + "\"");
info.Append(",\"is64bit\":" + (System.Environment.Is64BitOperatingSystem ? "true" : "false"));
try {
    string trustLevel = "unknown";
    var cfg = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
    var ts = cfg.GetSection("system.web/trust");
    if (ts != null) {
        var prop = ts.GetType().GetProperty("Level");
        if (prop != null) trustLevel = (string)prop.GetValue(ts);
    }
    info.Append(",\"trustLevel\":\"" + trustLevel + "\"");
} catch { info.Append(",\"trustLevel\":\"unavailable\""); }
try { info.Append(",\"mvc\":" + (Exists("System.Web.Mvc") ? "true" : "false")); } catch { info.Append(",\"mvc\":false"); }
try { info.Append(",\"webapi\":" + (Exists("System.Web.Http") ? "true" : "false")); } catch { info.Append(",\"webapi\":false"); }
try { info.Append(",\"routing\":" + (Exists("System.Web.Routing") ? "true" : "false")); } catch { info.Append(",\"routing\":false"); }
try {
    var vpp = System.Web.Hosting.HostingEnvironment.VirtualPathProvider;
    info.Append(",\"vpp\":\"" + (vpp == null ? "null" : vpp.GetType().FullName) + "\"");
} catch { info.Append(",\"vpp\":\"unknown\""); }
try { info.Append(",\"appPool\":\"" + System.Environment.GetEnvironmentVariable("APP_POOL_ID") + "\""); } catch {}
info.Append(",\"hosted\":" + (System.Web.Hosting.HostingEnvironment.IsHosted ? "true" : "false"));
info.Append("}");
Response.ContentType = "application/json";
Response.Write(info.ToString());
%>
<script runat="server">
static bool Exists(string typeName)
{
    try
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try { if (asm.GetType(typeName, false) != null) return true; } catch {}
        }
        return false;
    }
    catch { return false; }
}
</script>