using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DandyDocSite.Controllers
{
	public class DocsController : Controller
	{

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Api(string cref) {
			if (String.IsNullOrEmpty(cref))
				return View("Api/Index.cshtml");
			return new HttpNotFoundResult();
		}

    }
}
