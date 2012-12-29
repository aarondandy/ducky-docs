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
			if(String.IsNullOrEmpty(cref))
				return new HttpNotFoundResult();
			var reference = CrefOverlay.GetReference(cref);
			if (null == reference)
				return new HttpNotFoundResult();

			var typeDefinition = reference as TypeDefinition;
			if (null != typeDefinition) {
				return View("Type", new TypeViewModel(typeDefinition, XmlDocOverlay));
			}
			var methodDefinition = reference as MethodDefinition;
			if (null != methodDefinition){
				return View("Method", new MethodViewModel(methodDefinition, XmlDocOverlay));
			}
			var fieldDefinition = reference as FieldDefinition;
			if (null != fieldDefinition) {
				return View("Field", new FieldViewModel(fieldDefinition, XmlDocOverlay));
			}
			var propertyDefinition = reference as PropertyDefinition;
			if (null != propertyDefinition) {
				return View("Property", new PropertyViewModel(propertyDefinition, XmlDocOverlay));
			}
			var eventDefinition = reference as EventDefinition;
			if (null != eventDefinition) {
				return View("Event", new EventViewModel(eventDefinition, XmlDocOverlay));
			}

			throw new NotSupportedException();
		}

	}
}
