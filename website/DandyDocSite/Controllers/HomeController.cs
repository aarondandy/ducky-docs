using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DandyDocSite.Controllers
{
	public class HomeController : Controller
	{

		public ActionResult Index() {
			return View();
		}

		public ActionResult Examples() {
			return View();
		}

		public ActionResult Get() {
			return View();
		}

	}
}
