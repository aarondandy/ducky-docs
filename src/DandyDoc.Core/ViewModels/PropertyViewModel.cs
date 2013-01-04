using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

		public override string SubTitle { get { return "Property"; } }

		public ParsedXmlElementBase ValueDoc {
			get { return null == XmlDoc ? null : XmlDoc.ValueDoc; }
		}

		protected override IEnumerable<MemberFlair> GetFlairTags() {
			foreach (var tag in base.GetFlairTags())
				yield return tag;

			if (IsPure)
				yield return new MemberFlair("pure", "Purity", "Does not have side effects");

			var getMethod = Definition.GetMethod;
			var setMethod = Definition.SetMethod;
			var propertyVisibility = ExternalVisibilityOverlay.Get(Definition);

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

			if (Definition.IsSealed())
				yield return new MemberFlair("sealed", "Inheritance", "This property is sealed, preventing inheritance.");

			if(Definition.IsAbstract())
				yield return new MemberFlair("abstract", "Inheritance", "This property is abstract and must be implemented by inheriting types.");
			else if (Definition.IsVirtual() && Definition.IsNewSlot() && !Definition.IsFinal())
				yield return new MemberFlair("virtual", "Inheritance", "This method is virtual and can be overridden by inheriting types.");

			var getExposed = null != getMethod && getMethod.IsExternallyVisible();
			var setExposed = null != setMethod && setMethod.IsExternallyVisible();
			if (getExposed && setExposed) {
				if(GetViewModel.AllResultsAndParamsNotNull && SetViewModel.AllResultsAndParamsNotNull)
					yield return new MemberFlair("no nulls", "Null Values", "This property does not return or accept null.");
			}
			else if (getExposed) {
				if(GetViewModel.AllResultsAndParamsNotNull)
					yield return new MemberFlair("no nulls", "Null Values", "This property does not return null.");
			}
			else if (setExposed) {
				if(SetViewModel.AllResultsAndParamsNotNull)
					yield return new MemberFlair("no nulls", "Null Values", "This property does not accept null.");
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

		public MethodDefinitionXmlDoc GetterDocs {
			get { return null == XmlDoc ? null : XmlDoc.GetterDocs; }
		}

		public MethodDefinitionXmlDoc SetterDocs {
			get { return null == XmlDoc ? null : XmlDoc.SetterDocs; }
		}

		public MethodViewModel GetViewModel {
			get {
				if(null == Definition.GetMethod) throw new InvalidOperationException("Property has no getter.");
				Contract.EndContractBlock();
				return new MethodViewModel(Definition.GetMethod, XmlDocOverlay, CrefOverlay, GetterDocs);
			}
		}

		public MethodViewModel SetViewModel {
			get {
				if (null == Definition.SetMethod) throw new InvalidOperationException("Property has no setter.");
				Contract.EndContractBlock();
				return new MethodViewModel(Definition.SetMethod, XmlDocOverlay, CrefOverlay, SetterDocs);
			}
		}

	}
}
