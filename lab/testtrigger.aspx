<%@ Page Language="C#" %>
<%
Response.Write("ContentType: [" + Request.ContentType + "]<br>");
Response.Write("Form keys: ");
foreach (string k in Request.Form.AllKeys)
{
    Response.Write("[" + k + "]=[" + Request.Form[k] + "] ");
}
Response.Write("<br>");
Response.Write("Request[p@ss123]: [" + (Request["p@ss123"] == null ? "NULL" : Request["p@ss123"]) + "]<br>");
%>
