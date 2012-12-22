using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Mvc4WebDirectDocSample.App_Start;

namespace Mvc4WebDirectDocSample
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801
	public class MvcApplication : System.Web.HttpApplication
	{

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();
			DependencyResolver.SetResolver(new StructureMapConfig.Resolver(StructureMapConfig.Init()));
			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}
	}
}