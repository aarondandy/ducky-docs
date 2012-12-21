	using System;
using System.Collections.Generic;
using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace VertesaurMvcDoc
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Type",
				url: "Type",
				defaults: new{controller = "Doc", action = "Type"}
			);

			routes.MapRoute(
				name: "Member",
				url: "Member",
				defaults: new { controller = "Doc", action = "Type" }
			);

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);

		}
	}
}