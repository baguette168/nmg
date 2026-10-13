<%@ Page Language="C#" %>
<%
var vpp = System.Web.Hosting.HostingEnvironment.VirtualPathProvider;
if (vpp == null)
{
    Response.Write("VPP: NULL<br>");
}
else
{
    Response.Write("VPP type: " + vpp.GetType().FullName + "<br>");
    Response.Write("VPP assembly: " + vpp.GetType().Assembly.FullName + "<br>");
    Response.Write("Is default: " + (vpp.GetType() == typeof(System.Web.Hosting.VirtualPathProvider)).ToString() + "<br>");
}
Response.Write("CLR: " + System.Environment.Version.ToString() + "<br>");
%>
