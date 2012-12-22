using System;
using System.Web.Mvc;
using DandyDoc.Core;
using DandyDoc.Core.Overlays.Cref;

namespace Mvc4WebDirectDocSample.Controllers
{
	public class DocController : Controller
	{

		public DocController(
			AssemblyDefinitionCollection assemblyDefinitionCollection,
			CrefOverlay crefOverlay
		) {
			AssemblyDefinitionCollection = assemblyDefinitionCollection;
			CrefOverlay = crefOverlay;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public ActionResult Type(string cref) {
			if(String.IsNullOrEmpty(cref)) return new HttpNotFoundResult();
			var typeDefinition = CrefOverlay.GetTypeDefinition(cref);
			return View(typeDefinition);
		}

	}
}
