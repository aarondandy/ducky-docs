using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		public class AccessorViewModel
		{

			private readonly Lazy<ReadOnlyCollection<MemberFlair>> _flair;

			internal AccessorViewModel(MethodViewModel accessor, PropertyViewModel parent) {
				Contract.Requires(null != accessor);
				Contract.Requires(null != parent);
				Accessor = accessor;
				Parent = parent;
				_flair = new Lazy<ReadOnlyCollection<MemberFlair>>(
					() => new ReadOnlyCollection<MemberFlair>(
						new ReadOnlyCollection<MemberFlair>(Accessor.Flair
							.Where(f => Parent.Flair.All(x => x.Id != f.Id))
							.ToList()
						)
					)
				);
			}

			public MethodViewModel Accessor { get; private set; }

			public PropertyViewModel Parent { get; private set; }

			public IList<MemberFlair> Flair {
				get { return _flair.Value; }
			}

			public bool HasFlair {
				get { return Flair.Count > 0; }
			}

			public bool WorthDisplaying {
				get {
					return HasFlair
						|| Accessor.HasExceptions
						|| Accessor.HasRequires
						|| Accessor.HasEnsures;
				}
			}

			[ContractInvariantMethod]
			private void CodeContractInvariant(){
				Contract.Invariant(Accessor != null);
				Contract.Invariant(Parent != null);
			}

		}

		public PropertyViewModel(PropertyDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null)
			: base(definition, xmlDocOverlay, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
		}

		new public PropertyDefinitionXmlDoc XmlDoc { get { return (PropertyDefinitionXmlDoc)(base.XmlDoc); } }

		public override string SubTitle { get { return "Property"; } }

		public virtual ParsedXmlElementBase ValueDoc {
			get { return null == XmlDoc ? null : XmlDoc.ValueDoc; }
		}

		public virtual bool HasValueDoc{ get { return ValueDoc != null; } }

		protected override IEnumerable<MemberFlair> GetFlairTags() {
			foreach (var tag in base.GetFlairTags())
				yield return tag;

			var getMethod = Definition.GetMethod;
			var setMethod = Definition.SetMethod;

			var getExposed = null != getMethod && getMethod.IsExternallyVisible();
			var setExposed = null != setMethod && setMethod.IsExternallyVisible();

			if (getExposed && setExposed) {
				if (GetAccessorViewModel().Accessor.AllResultsAndParamsNotNull && SetAccessorViewModel().Accessor.AllResultsAndParamsNotNull)
					yield return new MemberFlair("no nulls", "Null Values", "This property does not return or accept null.");
			}
			else if (getExposed) {
				if (GetAccessorViewModel().Accessor.AllResultsAndParamsNotNull)
					yield return new MemberFlair("no nulls", "Null Values", "This property does not return null.");
			}
			else if (setExposed){
				if (SetAccessorViewModel().Accessor.AllResultsAndParamsNotNull)
					yield return new MemberFlair("no nulls", "Null Values", "This property does not accept null.");
			}

			if (IsPure)
				yield return new MemberFlair("pure", "Purity", "Does not have side effects");

			if (Definition.IsSealed())
				yield return new MemberFlair("sealed", "Inheritance", "This property is sealed, preventing inheritance.");

			if (!Definition.DeclaringType.IsInterface){
				if (Definition.IsAbstract())
					yield return new MemberFlair("abstract", "Inheritance", "This property is abstract and must be implemented by inheriting types.");
				else if (Definition.IsVirtual() && Definition.IsNewSlot() && !Definition.IsFinal())
					yield return new MemberFlair("virtual", "Inheritance", "This method is virtual and can be overridden by inheriting types.");
			}

			if(Definition.HasParameters && "Item".Equals(Definition.Name))
				yield return new MemberFlair("indexer", "Operator", "This property is invoked through a language index operator.");


			var propertyVisibility = ExternalVisibilityOverlay.Get(Definition);
			if (null != getMethod) {
				var methodVisibility = ExternalVisibilityOverlay.Get(getMethod);
				if (methodVisibility == propertyVisibility || methodVisibility == ExternalVisibilityKind.Public) {
					yield return new MemberFlair("get", "Property", "Value can be read externally.");
				}
				else if (methodVisibility == ExternalVisibilityKind.Protected) {
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

		}

		public virtual bool IsPure {
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

		public virtual IList<ParsedXmlException> Exceptions {
			get { return null == XmlDoc ? null : XmlDoc.Exceptions; }
		}

		public virtual bool HasExceptions {
			get {
				var exceptions = Exceptions;
				return null != exceptions && exceptions.Count > 0;
			}
		}

		public virtual IEnumerable<ExceptionGroupViewModel> ExceptionGroups {
			get {
				if (!HasExceptions) throw new InvalidOperationException("Model has no exceptions.");
				Contract.Ensures(Contract.Result<IEnumerable<ExceptionGroupViewModel>>() != null);
				Contract.Assume(null != Exceptions);
				return ToExceptionGroupViewModels(ToExceptionViewModels(Exceptions));
			}
		}

		public virtual bool IsGetterPure {
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

		public virtual bool HasExposedGet {
			get { return Definition.GetMethod != null && Definition.GetMethod.IsExternallyVisible(); }
		}

		public virtual bool HasProtectedGet {
			get { return Definition.GetMethod != null && Definition.GetMethod.IsExternallyProtected(); }
		}

		public virtual bool HasExposedSet {
			get { return Definition.SetMethod != null && Definition.SetMethod.IsExternallyVisible(); }
		}

		public virtual bool HasProtectedSet {
			get { return Definition.SetMethod != null && Definition.SetMethod.IsExternallyProtected(); }
		}

		public virtual MethodDefinitionXmlDoc GetterDocs {
			get { return null == XmlDoc ? null : XmlDoc.GetterDocs; }
		}

		public virtual MethodDefinitionXmlDoc SetterDocs {
			get { return null == XmlDoc ? null : XmlDoc.SetterDocs; }
		}

		public virtual AccessorViewModel GetAccessorViewModel() {
			if(null == Definition.GetMethod) throw new InvalidOperationException("Property has no getter.");
			Contract.EndContractBlock();
			return new AccessorViewModel(
				new MethodViewModel(Definition.GetMethod, XmlDocOverlay, CrefOverlay, GetterDocs),
				this);
		}

		public virtual AccessorViewModel SetAccessorViewModel() {
			if (null == Definition.SetMethod) throw new InvalidOperationException("Property has no setter.");
			Contract.EndContractBlock();
			return new AccessorViewModel(
				new MethodViewModel(Definition.SetMethod, XmlDocOverlay, CrefOverlay, SetterDocs),
				this);
		}

	}
}
