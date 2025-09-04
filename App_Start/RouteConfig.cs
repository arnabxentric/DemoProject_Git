using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace XenERP
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            string url1 = "DoubleEntry/Receive/{id}";
            string url2 = "DoubleEntry/Payment/{id}";
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("DoubleEntry/CreateDEV");
            routes.IgnoreRoute("DoubleEntry/CreateDEV/{id}");
            //foreach (string url in urls)
            //    routes.MapRoute("RouteName-" + url, url, new { controller = "Page", action = "Index" });

            routes.MapRoute("Receive", url1, new { controller = "DoubleEntry", action = "CreateDEV", id = UrlParameter.Optional, msg = UrlParameter.Optional });
            routes.MapRoute("Payment", url2, new { controller = "DoubleEntry", action = "CreateDEV", id = UrlParameter.Optional, msg = UrlParameter.Optional });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}