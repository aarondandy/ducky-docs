using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.CodeSignature;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.ExternalVisibility;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{

	public abstract class DefinitionViewModelBase<TDefinition> : IDefinitionViewModel
		where TDefinition : class, IMemberDefinition
	{

		protected static readonly DisplayNameOverlay ShortNameOverlay = new DisplayNameOverlay();

		protected static string GetShortName(IMemberDefinition definition){
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return ShortNameOverlay.GetDisplayName(definition);
		}

		private readonly Lazy<DefinitionXmlDocBase> _xmlDoc;
		private readonly Lazy<ReadOnlyCollection<MemberFlair>> _flair;
		private readonly Lazy<ReadOnlyCollection<CodeSignature>> _signatures;

		protected DefinitionViewModelBase(TDefinition definition, TypeViewModel typeViewModelContainer, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null) {
			if (null == definition) throw new ArgumentNullException("definition");
			if (null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			Definition = definition;
			TypeViewModelContainer = typeViewModelContainer;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
			_xmlDoc = new Lazy<DefinitionXmlDocBase>(() => XmlDocOverlay.GetDocumentation(Definition));
			_flair = new Lazy<ReadOnlyCollection<MemberFlair>>(() => {
				var results = GetFlairTags();
				return null == results ? MemberFlair.EmptyList : Array.AsReadOnly(results.ToArray());
			});
			_signatures = new Lazy<ReadOnlyCollection<CodeSignature>>(() => new ReadOnlyCollection<CodeSignature>(new CodeSignatureOverlay().GenerateSignatures(Definition)));
		}

		public TDefinition Definition { get; private set; }

		IMemberDefinition IDefinitionViewModel.Definition { get { return Definition; } }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public virtual string Cref { get { return CrefOverlay.GetCref(Definition); } }

		public virtual DefinitionXmlDocBase XmlDoc { get { return _xmlDoc.Value; } }

		public virtual bool HasXmlDoc { get { return XmlDoc != null; } }

		public virtual IList<CodeSignature> Signatures { get { return _signatures.Value; } }

		public virtual bool HasSignatures { get { return CollectionUtility.IsNotNullOrEmpty(Signatures); } }

		public virtual TypeViewModel TypeViewModelContainer { get; private set; }

		public virtual bool MemberDeclaredInAnotherType {
			get{
				if (null == TypeViewModelContainer)
					return false;

				var containerType = TypeViewModelContainer.Definition;
				var declaringType = Definition.DeclaringType;
				if (containerType == declaringType)
					return false;

				return true;
			}
		}

		protected virtual MemberFlair VisibilityFlair {
			get {
				Contract.Ensures(Contract.Result<MemberFlair>() != null);
				switch (ExternalVisibilityOverlay.Get(Definition)) {
					case ExternalVisibilityKind.Hidden: return new MemberFlair("hidden","Visibility","Not externally visible.");
					case ExternalVisibilityKind.Protected: return new MemberFlair("protected","Visibility", "Externally visible only through inheritance.");
					case ExternalVisibilityKind.Public: return new MemberFlair("public","Visibility", "Externally visible.");
					default: throw new InvalidOperationException("This visibility level is not supported.");
				}
			}
		}

		protected virtual IEnumerable<MemberFlair> GetFlairTags() {
			yield return VisibilityFlair;

			if (Definition.IsStatic())
				yield return new MemberFlair("static", "Static", "Accessible relative to a type rather than an object instance.");

			if (Definition.HasObsoleteAttribute())
				yield return new MemberFlair("obsolete", "Warning", "This is deprecated.");
		}

		public IList<MemberFlair> Flair {
			get {
				Contract.Ensures(Contract.Result<IList<string>>() != null);
				return _flair.Value;
			}
		}

		public bool HasFlair { get { return CollectionUtility.IsNotNullOrEmpty(Flair); } }

		public virtual ParsedXmlElementBase Summary { get { return null == XmlDoc ? null : XmlDoc.Summary; } }

		public virtual bool HasSummary { get { return Summary != null; } }

		public virtual IList<ParsedXmlElementBase> Remarks { get { return null == XmlDoc ? null : XmlDoc.Remarks; } }

		public virtual bool HasRemarks { get { return CollectionUtility.IsNotNullOrEmpty(Remarks); } }

		public virtual IList<ParsedXmlElementBase> Examples { get { return null == XmlDoc ? null : XmlDoc.Examples; } }

		public virtual bool HasExamples { get { return CollectionUtility.IsNotNullOrEmpty(Examples); } }

		public virtual IList<ParsedXmlSeeElement> SeeAlso { get { return null == XmlDoc ? null : XmlDoc.SeeAlso; } }

		public virtual bool HasSeeAlso { get { return CollectionUtility.IsNotNullOrEmpty(SeeAlso); } }

		public virtual AssemblyNamespaceViewModel AssemblyNamespace {
			get {
				Contract.Ensures(Contract.Result<AssemblyNamespaceViewModel>() != null);
				return new AssemblyNamespaceViewModel(Definition);
			}
		}

		public virtual string Title {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				var name = ShortName;
				var declaringType = Definition.DeclaringType;
				if (null != declaringType) {
					name = String.Concat(GetShortName(declaringType), '.', name);
				}
				return name;
			}
		}

		public abstract string SubTitle { get; }

		public virtual string ShortName{
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return ShortNameOverlay.GetDisplayName(Definition);
			}
		}

		public IEnumerable<ExceptionViewModel> ToExceptionViewModels(IEnumerable<ParsedXmlException> exceptions){
			if(null == exceptions) throw new ArgumentNullException("exceptions");
			Contract.Ensures(Contract.Result<IEnumerable<ExceptionViewModel>>() != null);
			return exceptions.Select(ex => new ExceptionViewModel(ex));
		}

		public IEnumerable<ExceptionGroupViewModel> ToExceptionGroupViewModels(IEnumerable<ExceptionViewModel> exceptions) {
			if(null == exceptions) throw new ArgumentNullException("exceptions");
			Contract.Ensures(Contract.Result<IEnumerable<ExceptionGroupViewModel>>() != null);
			return exceptions
				.GroupBy(x => x.ExceptionXml.CRef)
				.Select(x => new ExceptionGroupViewModel(x.ToList()));
		}
			
		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Definition);
			Contract.Invariant(null != XmlDocOverlay);
			Contract.Invariant(null != CrefOverlay);
		}

	}
}
