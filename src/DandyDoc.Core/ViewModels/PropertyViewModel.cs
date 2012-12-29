using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
			if (null != getMethod) {
				var methodVisibility = ExternalVisibilityOverlay.Get(getMethod);
				if (methodVisibility == propertyVisibility || methodVisibility == ExternalVisibilityKind.Public) {
					yield return "get";
				}
				else if(methodVisibility == ExternalVisibilityKind.Protected) {
					yield return "proget";
				}
			}

			var setMethod = Definition.SetMethod;
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

		public bool HasExposedGet {
			get {
				return Definition.GetMethod != null && Definition.GetMethod.IsExternallyVisible();
			}
		}

		public bool HasExposedSet {
			get {
				return Definition.SetMethod != null && Definition.SetMethod.IsExternallyVisible();
			}
		}

	}
}
