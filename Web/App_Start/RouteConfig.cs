using System.Web.Mvc;
using System.Web.Routing;

namespace Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "DeleteTweetTest",
                url: "test/delete/{id}",
                defaults: new {controller = "Test", action = "Delete"}
                );

            routes.MapRoute(
                name: "RateTweetTest",
                url: "test/{id}/{sentiment}",
                defaults: new {controller = "Test", action = "RateTweet"}
                );

            routes.MapRoute(
                name: "DeleteTweet",
                url: "training/delete/{id}",
                defaults: new {controller = "Training", action = "Delete"}
                );

            routes.MapRoute(
                name: "RateTweet",
                url: "training/{id}/{sentiment}",
                defaults: new {controller = "Training", action = "RateTweet"}
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Overview", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}