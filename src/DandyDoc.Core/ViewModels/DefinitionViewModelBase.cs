using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.ExternalVisibility;
using DandyDoc.Core.Overlays.XmlDoc;
using DandyDoc.Core.Utility;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{

	public abstract class DefinitionViewModelBase<TDefinition> : IDefinitionViewModel
		where TDefinition : IMemberDefinition
	{

		private readonly Lazy<DefinitionXmlDocBase> _xmlDoc;
		private readonly Lazy<ReadOnlyCollection<string>> _flair; 

		protected DefinitionViewModelBase(TDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null) {
			if (null == definition) throw new ArgumentNullException("definition");
			if (null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			Definition = definition;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
			_xmlDoc = new Lazy<DefinitionXmlDocBase>(() => XmlDocOverlay.GetDocumentation(Definition));
			_flair = new Lazy<ReadOnlyCollection<string>>(() => {
				var results = GetFlairTags();
				return null == results
					? CollectionUtility.EmptyStringCollection
					: Array.AsReadOnly(results.ToArray());
			});
		}

		public TDefinition Definition { get; private set; }

		IMemberDefinition IDefinitionViewModel.Definition { get { return Definition; } }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public string Cref { get { return CrefOverlay.GetCref(Definition); } }

		public DefinitionXmlDocBase XmlDoc { get { return _xmlDoc.Value; } }

		public bool HasXmlDoc { get { return XmlDoc != null; } }

		protected virtual IEnumerable<string> GetFlairTags() {
			var xmlDoc = XmlDoc;

			yield return ExternalVisibilityOverlay.Get(Definition).ToString().ToLowerInvariant();
			if (null != xmlDoc && XmlDoc.HasPureElement) // TODO: ... or has a pure attribute
				yield return "pure";

			if (Definition.IsStatic())
				yield return "static";
		}

		public ReadOnlyCollection<string> Flair {
			get {
				Contract.Ensures(Contract.Result<ReadOnlyCollection<string>>() != null);
				return _flair.Value;
			}
		}

		IList<string> IDefinitionViewModel.Flair { get { return Flair; } } 

		public ParsedXmlElementBase Summary {
			get { return null == XmlDoc ? null : XmlDoc.Summary; }
		}

		public bool HasSummary { get { return Summary != null; } }

		public ParsedXmlElementBase Remarks {
			get { return null == XmlDoc ? null : XmlDoc.Remarks; }
		}

		public IList<ParsedXmlElementBase> Examples {
			get { return null == XmlDoc ? null : XmlDoc.Examples; }
		}

		public IList<ParsedXmlSeeElement> SeeAlso {
			get { return null == XmlDoc ? null : XmlDoc.SeeAlso; }
		}

		public AssemblyNamespaceViewModel AssemblyNamespace {
			get {
				Contract.Ensures(Contract.Result<AssemblyNamespaceViewModel>() != null);
				return new AssemblyNamespaceViewModel(Definition);
			}
		}

		public abstract string Title { get; }

		public abstract string ShortName { get; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Definition);
			Contract.Invariant(null != XmlDocOverlay);
			Contract.Invariant(null != CrefOverlay);
		}

	}
}
