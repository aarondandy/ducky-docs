using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DandyDocSite.Infrastructure;

namespace DandyDocSite.Controllers
{
	public class DocsController : Controller
	{

		public DocsController(ApiDocNavigation navigation){
			Navigation = navigation;
		}

		public ApiDocNavigation Navigation { get; set; }

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Api(string cref){
			ViewBag.TypeNavigationViewModel = Navigation.NavigationRepository;

			if (String.IsNullOrEmpty(cref))
				return View("Api/Index", Navigation.NavigationRepository);

			return new HttpNotFoundResult();
		}

    }
}
