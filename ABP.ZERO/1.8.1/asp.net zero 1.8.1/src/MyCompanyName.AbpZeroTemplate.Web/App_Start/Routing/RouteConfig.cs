using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace MyCompanyName.AbpZeroTemplate.Web.Routing
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //ASP.NET Web API Route Config
            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            //配置MVC路由表，URL路由的映射模式
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                //MVC根据{controller}在应用程序集中搜索同名控制类，如果在不同命名空间下有同名的控制类，MVC会给出多个同名控制类的异常
                //我们可以在注册路由的时候指定搜索的命令空间,可以添加多个命名空间
                namespaces: new[] { "MyCompanyName.AbpZeroTemplate.Web.Controllers" }
            );
        }
    }
}
