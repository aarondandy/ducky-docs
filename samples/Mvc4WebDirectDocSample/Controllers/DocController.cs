using System;
using System.Diagnostics.Contracts;
using System.Web.Mvc;
using DandyDoc;
using DandyDoc.Overlays.Cref;
using DandyDoc.ViewModels;
using Mono.Cecil;
using DandyDoc.Overlays.XmlDoc;

namespace Mvc4WebDirectDocSample.Controllers
{
	public class DocController : Controller
	{

		public DocController(
			AssemblyDefinitionCollection assemblyDefinitionCollection,
			CrefOverlay crefOverlay,
			XmlDocOverlay xmlDocOverlay,
			TypeNavigationViewModel typeNavigationViewModel
		) {
			AssemblyDefinitionCollection = assemblyDefinitionCollection;
			CrefOverlay = crefOverlay;
			XmlDocOverlay = xmlDocOverlay;
			TypeNavigationViewModel = typeNavigationViewModel;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public TypeNavigationViewModel TypeNavigationViewModel { get; private set; }

		public ActionResult Index(string cref) {
			if(String.IsNullOrEmpty(cref))
				return new HttpNotFoundResult();
			var reference = CrefOverlay.GetReference(cref);
			if (null == reference)
				return new HttpNotFoundResult();

			ViewResult viewResult;
			if (reference is TypeDefinition){
				var typeDefinition = (TypeDefinition) reference;
				if(typeDefinition.IsEnum)
					viewResult = View("Enum", new TypeViewModel(typeDefinition, XmlDocOverlay));
				else
					viewResult = View("Type", new TypeViewModel(typeDefinition, XmlDocOverlay));
			}
			else if (reference is MethodDefinition) {
				viewResult = View("Method", new MethodViewModel((MethodDefinition)reference, XmlDocOverlay));
			}
			else if (reference is FieldDefinition) {
				viewResult = View("Field", new FieldViewModel((FieldDefinition)reference, XmlDocOverlay));
			}
			else if (reference is PropertyDefinition) {
				viewResult = View("Property", new PropertyViewModel((PropertyDefinition)reference, XmlDocOverlay));
			}
			else if (reference is EventDefinition){
				viewResult = View("Event", new EventViewModel((EventDefinition) reference, XmlDocOverlay));
			}
			else{
				throw new NotSupportedException();
			}
			Contract.Assume(null != viewResult);
			viewResult.ViewBag.TypeNavigationViewModel = TypeNavigationViewModel;
			return viewResult;
		}

	}
}
