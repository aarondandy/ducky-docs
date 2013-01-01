using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.ExternalVisibility;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
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

		public override string Title { get { return base.Title + " Property"; } }

		public ParsedXmlElementBase ValueDoc {
			get { return null == XmlDoc ? null : XmlDoc.ValueDoc; }
		}

		protected override IEnumerable<MemberFlair> GetFlairTags() {
			foreach (var tag in base.GetFlairTags())
				yield return tag;

			var propertyVisibility = ExternalVisibilityOverlay.Get(Definition);

			var getMethod = Definition.GetMethod;
			var setMethod = Definition.SetMethod;

			if (IsPure) {
				yield return new MemberFlair("pure", "Purity", "Does not have side effects");
			}

			if (null != getMethod) {
				var methodVisibility = ExternalVisibilityOverlay.Get(getMethod);
				if (methodVisibility == propertyVisibility || methodVisibility == ExternalVisibilityKind.Public) {
					yield return new MemberFlair("get", "Property", "Value can be read externally.");
				}
				else if(methodVisibility == ExternalVisibilityKind.Protected) {
					yield return new MemberFlair("proget", "Property", "Value can be read through inheritance.");
				}
			}

			if (null != setMethod) {
				var methodVisibility = ExternalVisibilityOverlay.Get(setMethod);
				if (methodVisibility == propertyVisibility || methodVisibility == ExternalVisibilityKind.Public) {
					yield return new MemberFlair("set", "Property", "Value can be assigned externally.");
				}
				else if (methodVisibility == ExternalVisibilityKind.Protected) {
					yield return new MemberFlair("proset", "Property", "Value can be assigned through inheritance.");
				}
			}

			if(Definition.HasParameters && "Item".Equals(Definition.Name))
				yield return new MemberFlair("indexer", "Operator", "This property is invoked through a language index operator.");

			var getSealed = null != getMethod && getMethod.IsFinal;
			var setSealed = null != setMethod && setMethod.IsFinal;
			if (getSealed || setSealed)
				yield return new MemberFlair("sealed", "Inheritance", " This property is sealed, preventing inheritance.");
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

		public IList<ParsedXmlException> Exceptions {
			get { return null == XmlDoc ? null : XmlDoc.Exceptions; }
		}

		public bool HasExceptions {
			get {
				var exceptions = Exceptions;
				return null != exceptions && exceptions.Count > 0;
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
				if(null == Definition.GetMethod) throw new InvalidOperationException("Property has no getter.");
				Contract.EndContractBlock();
				return new MethodViewModel(Definition.GetMethod, XmlDocOverlay, CrefOverlay);
			}
		}

		public MethodViewModel SetViewModel {
			get {
				if (null == Definition.SetMethod) throw new InvalidOperationException("Property has no setter.");
				Contract.EndContractBlock();
				return new MethodViewModel(Definition.SetMethod, XmlDocOverlay, CrefOverlay);
			}
		}

	}
}
