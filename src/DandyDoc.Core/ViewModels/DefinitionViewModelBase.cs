using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.ExternalVisibility;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{

	public abstract class DefinitionViewModelBase<TDefinition> : IDefinitionViewModel
		where TDefinition : IMemberDefinition
	{

		private static readonly DisplayNameOverlay ShortNameOverlay = new DisplayNameOverlay();

		protected static string GetShortName(IMemberDefinition definition){
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return ShortNameOverlay.GetDisplayName(definition);
		}

		private readonly Lazy<DefinitionXmlDocBase> _xmlDoc;
		private readonly Lazy<ReadOnlyCollection<MemberFlair>> _flair; 

		protected DefinitionViewModelBase(TDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null) {
			if (null == definition) throw new ArgumentNullException("definition");
			if (null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			Definition = definition;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
			_xmlDoc = new Lazy<DefinitionXmlDocBase>(() => XmlDocOverlay.GetDocumentation(Definition));
			_flair = new Lazy<ReadOnlyCollection<MemberFlair>>(() => {
				var results = GetFlairTags();
				return null == results ? MemberFlair.EmptyList : Array.AsReadOnly(results.ToArray());
			});
		}

		public TDefinition Definition { get; private set; }

		IMemberDefinition IDefinitionViewModel.Definition { get { return Definition; } }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public virtual string Cref { get { return CrefOverlay.GetCref(Definition); } }

		public DefinitionXmlDocBase XmlDoc { get { return _xmlDoc.Value; } }

		public bool HasXmlDoc { get { return XmlDoc != null; } }

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
		}

		public IList<MemberFlair> Flair {
			get {
				Contract.Ensures(Contract.Result<IList<string>>() != null);
				return _flair.Value;
			}
		} 

		public ParsedXmlElementBase Summary {
			get { return null == XmlDoc ? null : XmlDoc.Summary; }
		}

		public bool HasSummary { get { return Summary != null; } }

		public ParsedXmlElementBase Remarks {
			get { return null == XmlDoc ? null : XmlDoc.Remarks; }
		}

		public bool HasRemarks { get { return Remarks != null; } }

		public IList<ParsedXmlElementBase> Examples {
			get { return null == XmlDoc ? null : XmlDoc.Examples; }
		}

		public bool HasExamples{
			get{
				var examples = Examples;
				return null != examples && examples.Count > 0;
			}
		}

		public IList<ParsedXmlSeeElement> SeeAlso {
			get { return null == XmlDoc ? null : XmlDoc.SeeAlso; }
		}

		public bool HasSeeAlso{
			get {
				var seeAlso = SeeAlso;
				return null != seeAlso && seeAlso.Count > 0;
			}
		}

		public AssemblyNamespaceViewModel AssemblyNamespace {
			get {
				Contract.Ensures(Contract.Result<AssemblyNamespaceViewModel>() != null);
				return new AssemblyNamespaceViewModel(Definition);
			}
		}

		public virtual string Title {
			get{
				var name = ShortName;
				var declaringType = Definition.DeclaringType;
				if (null != declaringType) {
					name = String.Concat(GetShortName(declaringType), '.', name);
				}
				return name;
			}
		}

		public virtual string ShortName { get { return ShortNameOverlay.GetDisplayName(Definition); } }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Definition);
			Contract.Invariant(null != XmlDocOverlay);
			Contract.Invariant(null != CrefOverlay);
		}

	}
}
