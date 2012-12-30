using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.ExternalVisibility;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class PropertyViewModel : DefinitionViewModelBase<PropertyDefinition>
	{

		public PropertyViewModel(PropertyDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null)
			: base(definition, xmlDocOverlay, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
		}

		new public PropertyDefinitionXmlDoc XmlDoc { get { return (PropertyDefinitionXmlDoc)(base.XmlDoc); } }

		public override string Title { get { return Definition.Name + " Property"; } }

		public override string ShortName { get { return Definition.Name; } }

		public ParsedXmlElementBase ValueDoc {
			get { return null == XmlDoc ? null : XmlDoc.ValueDoc; }
		}

		protected override IEnumerable<string> GetFlairTags() {
			foreach (var tag in base.GetFlairTags())
				yield return tag;

			var propertyVisibility = ExternalVisibilityOverlay.Get(Definition);

			var getMethod = Definition.GetMethod;
			var setMethod = Definition.SetMethod;

			if (IsPure) {
				yield return "pure";
			}

			if (null != getMethod) {
				var methodVisibility = ExternalVisibilityOverlay.Get(getMethod);
				if (methodVisibility == propertyVisibility || methodVisibility == ExternalVisibilityKind.Public) {
					yield return "get";
				}
				else if(methodVisibility == ExternalVisibilityKind.Protected) {
					yield return "proget";
				}
			}

			if (null != setMethod) {
				var methodVisibility = ExternalVisibilityOverlay.Get(setMethod);
				if (methodVisibility == propertyVisibility || methodVisibility == ExternalVisibilityKind.Public) {
					yield return "set";
				}
				else if (methodVisibility == ExternalVisibilityKind.Protected) {
					yield return "proset";
				}
			}
		}

		public bool IsPure {
			get {
				if (Definition.HasPureAttribute()) {
					return true;
				}
				if (HasExposedGet) {
					if (IsGetterPure)
						return true;
				}
				return false;
			}
		}

		public bool IsGetterPure {
			get {
				if (HasXmlDoc && null != XmlDoc.GetterDocs && XmlDoc.GetterDocs.HasPureElement)
					return true;
				if (null != Definition.GetMethod) {
					if (Definition.GetMethod.HasPureAttribute())
						return true;
				}
				return false;
			}
		}

		public bool HasExposedGet {
			get { return Definition.GetMethod != null && Definition.GetMethod.IsExternallyVisible(); }
		}

		public bool HasProtectedGet {
			get { return Definition.GetMethod != null && Definition.GetMethod.IsExternallyProtected(); }
		}

		public bool HasExposedSet {
			get { return Definition.SetMethod != null && Definition.SetMethod.IsExternallyVisible(); }
		}

		public bool HasProtectedSet {
			get { return Definition.SetMethod != null && Definition.SetMethod.IsExternallyProtected(); }
		}

		public MethodViewModel GetViewModel {
			get {
				return new MethodViewModel(Definition.GetMethod, XmlDocOverlay, CrefOverlay);
			}
		}

		public MethodViewModel SetViewModel {
			get {
				return new MethodViewModel(Definition.SetMethod, XmlDocOverlay, CrefOverlay);
			}
		}

	}
}
