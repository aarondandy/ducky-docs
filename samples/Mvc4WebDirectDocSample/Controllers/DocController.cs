using System;
using System.Diagnostics.Contracts;
using System.Web.Mvc;
using DandyDoc;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.MsdnLinks;
using DandyDoc.ViewModels;
using Mono.Cecil;
using DandyDoc.Overlays.XmlDoc;
using Mvc4WebDirectDocSample.Infrastructure;

namespace Mvc4WebDirectDocSample.Controllers
{
	public class DocController : Controller
	{

		public DocController(
			AssemblyDefinitionCollection assemblyDefinitionCollection,
			CRefOverlay cRefOverlay,
			XmlDocOverlay xmlDocOverlay,
			TypeNavigationViewModel typeNavigationViewModel,
			IMsdnLinkOverlay msdnLinkOverlay
		) {
			AssemblyDefinitionCollection = assemblyDefinitionCollection;
			CRefOverlay = cRefOverlay;
			XmlDocOverlay = xmlDocOverlay;
			TypeNavigationViewModel = typeNavigationViewModel;
			MsdnLinkOverlay = msdnLinkOverlay;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CRefOverlay CRefOverlay { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public TypeNavigationViewModel TypeNavigationViewModel { get; private set; }

		public IMsdnLinkOverlay MsdnLinkOverlay { get; private set; }

		public ActionResult Index(string cref) {
			if(String.IsNullOrEmpty(cref))
				return new HttpNotFoundResult();

			ViewResult viewResult;
			var parsedCref = new ParsedCref(cref);
			if ("N".Equals(parsedCref.TargetType)) {
				var ns = TypeNavigationViewModel.GetExposedNamespaceViewModel(parsedCref.CoreName);
				if(null == ns)
					return new HttpNotFoundResult();
				viewResult = View("Namespace", ns);
			}
			else {
				var reference = CRefOverlay.GetReference(parsedCref);
				if (null == reference)
					return new HttpNotFoundResult();
				if (reference is TypeDefinition) {
					var typeDefinition = (TypeDefinition)reference;
					if (typeDefinition.IsDelegateType()) {
						var delegateViewModel = new DelegateViewModel(typeDefinition, null, XmlDocOverlay);
						viewResult = View("Delegate", delegateViewModel);
					}
					else {
						var typeViewModel = new TypeViewModel(typeDefinition, null, XmlDocOverlay);
						viewResult = View(
							typeDefinition.IsEnum ? "Enum" : "Type",
							typeViewModel);
					}
				}
				else if (reference is MethodDefinition) {
					viewResult = View("Method", new MethodViewModel((MethodDefinition)reference, null, XmlDocOverlay));
				}
				else if (reference is FieldDefinition) {
					viewResult = View("Field", new FieldViewModel((FieldDefinition)reference, null, XmlDocOverlay));
				}
				else if (reference is PropertyDefinition) {
					viewResult = View("Property", new PropertyViewModel((PropertyDefinition)reference, null, XmlDocOverlay));
				}
				else if (reference is EventDefinition) {
					viewResult = View("Event", new EventViewModel((EventDefinition)reference, null, XmlDocOverlay));
				}
				else {
					throw new NotSupportedException();
				}
			}

			Contract.Assume(null != viewResult);

			viewResult.ViewBag.MemberDefinitionLinkResolver = new MemberDefinitionLinkResolver(
				CRefOverlay,
				Url,
				MsdnLinkOverlay
			);
			viewResult.ViewBag.TypeNavigationViewModel = TypeNavigationViewModel;

			return viewResult;
		}

	}
}
