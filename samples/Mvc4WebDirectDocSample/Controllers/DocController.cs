using System;
using System.Web.Mvc;
using DandyDoc.Core;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.ViewModels;
using Mono.Cecil;
using DandyDoc.Core.Overlays.XmlDoc;

namespace Mvc4WebDirectDocSample.Controllers
{
	public class DocController : Controller
	{

		public DocController(
			AssemblyDefinitionCollection assemblyDefinitionCollection,
			CrefOverlay crefOverlay,
			XmlDocOverlay xmlDocOverlay
		) {
			AssemblyDefinitionCollection = assemblyDefinitionCollection;
			CrefOverlay = crefOverlay;
			XmlDocOverlay = xmlDocOverlay;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public ActionResult Index(string cref) {
			if(String.IsNullOrEmpty(cref)) return new HttpNotFoundResult();
			var reference = CrefOverlay.GetReference(cref);
			if (null == reference){
				return new HttpNotFoundResult();
			}
			var typeDefinition = reference as TypeDefinition;
			if (null != typeDefinition) {
				/*return View("Type", new TypeViewModel{
					Definition = typeDefinition,
					XmlDocOverlay = XmlDocOverlay,
					CrefOverlay = CrefOverlay,
					XmlDoc = XmlDocOverlay.GetDocumentation(typeDefinition)
				});*/
				return View("Type", new TypePageViewModel(typeDefinition, XmlDocOverlay));
			}
			else if (reference is MethodDefinition){
				return View("Method", (MethodDefinition) reference);
			}
			else{
				throw new NotSupportedException();
			}

		}

	}
}
