using System.Web.Mvc;
using System.Web.Routing;

namespace DandyDocSite
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Docs",
				url: "Docs/{action}",
				defaults: new{controller = "Docs", action = "Index"});

			routes.MapRoute(
				name: "Default",
				url: "{action}",
				defaults: new { controller = "Home", action="Index"});

			/*routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);*/
		}
	}
}