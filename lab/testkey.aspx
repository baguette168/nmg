<%@ Page Language="C#" %>
<%
var vpp = System.Web.Hosting.HostingEnvironment.VirtualPathProvider;
Response.Write("VPP: " + vpp.GetType().FullName + "<br>");
try
{
    string key = vpp.GetCacheKey(Request.Path);
    Response.Write("GetCacheKey returned: " + (key ?? "NULL") + "<br>");
}
catch (Exception ex)
{
    Response.Write("GetCacheKey threw: " + ex.Message + "<br>");
    if (ex.InnerException != null)
        Response.Write("Inner: " + ex.InnerException.Message + "<br>");
}
try
{
    bool exists = vpp.FileExists(Request.Path);
    Response.Write("FileExists: " + exists + "<br>");
}
catch (Exception ex2)
{
    Response.Write("FileExists threw: " + ex2.Message + "<br>");
}
%>
