using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public abstract class DefinitionSimpleModelBase<TDefinition> : ISimpleModel
		where TDefinition : MemberReference, IMemberDefinition
	{

		private class XmlDocsDetails
		{

			private static readonly IComplexTextNode[] EmptyTextNodeList = new IComplexTextNode[0];

			public XmlDocsDetails() {
				HasPureElement = false;
				Examples = EmptyTextNodeList;
				Permissions = EmptyTextNodeList;
				Remarks = EmptyTextNodeList;
				SeeAlso = EmptyTextNodeList;
			}

			public XmlDocsDetails(DefinitionXmlDocBase core) {
				HasPureElement = core.HasPureElement;
				Examples = null == core.Examples ? EmptyTextNodeList : ParsedXmlDocComplexTextNode.Convert(core.Examples);
				Permissions = null == core.Permissions ? EmptyTextNodeList : ParsedXmlDocComplexTextNode.Convert(core.Permissions);
				Remarks = null == core.Remarks ? EmptyTextNodeList : ParsedXmlDocComplexTextNode.Convert(core.Remarks);
				SeeAlso = null == core.SeeAlso ? EmptyTextNodeList : ParsedXmlDocComplexTextNode.Convert(core.SeeAlso);
			}

			public bool HasPureElement { get; private set; }

			public IList<IComplexTextNode> Examples { get; private set; }

			public IList<IComplexTextNode> Permissions { get; private set; }

			public IList<IComplexTextNode> Remarks { get; private set; }

			public IList<IComplexTextNode> SeeAlso { get; private set; }

			[ContractInvariantMethod]
			private void CodeContractInvariant() {
				Contract.Invariant(Examples != null);
				Contract.Invariant(Permissions != null);
				Contract.Invariant(Remarks != null);
				Contract.Invariant(SeeAlso != null);
			}

		}

		protected static readonly IFlairTag DefaultStaticFlair = new SimpleFlairTag("static", "Static", "Accessible relative to a type rather than an object instance.");

		protected static readonly IFlairTag DefaultObsoleteTag = new SimpleFlairTag("obsolete", "Warning", "This is deprecated.");

		protected static readonly DisplayNameOverlay RegularTypeDisplayNameOverlay = new DisplayNameOverlay {
			ShowTypeNameForMembers = false
		};

		protected static readonly DisplayNameOverlay NestedTypeDisplayNameOverlay = new DisplayNameOverlay {
			ShowTypeNameForMembers = true
		};

		protected static readonly DisplayNameOverlay FullTypeDisplayNameOverlay = new DisplayNameOverlay {
			ShowTypeNameForMembers = true,
			IncludeNamespaceForTypes = true
		};

		private readonly Lazy<IComplexTextNode> _summaryDocs;
		private readonly Lazy<XmlDocsDetails> _detailsDocs;
		private readonly Lazy<DefinitionXmlDocBase> _xmlDocs;

		protected DefinitionSimpleModelBase(TDefinition definition, IAssemblySimpleModel assemblyModel) {
			if (null == definition) throw new ArgumentNullException("definition");
			if (null == assemblyModel) throw new ArgumentNullException("assemblyModel");
			Contract.EndContractBlock();
			Definition = definition;
			ContainingAssembly = assemblyModel;
			_xmlDocs = new Lazy<DefinitionXmlDocBase>(() => ContainingAssembly.XmlDocOverlay.GetDocumentation((MemberReference)Definition), true);
			_summaryDocs = new Lazy<IComplexTextNode>(CreateSummaryDocs, true);
			_detailsDocs = new Lazy<XmlDocsDetails>(CreateDetailsDocs, true);
		}

		private XmlDocsDetails CreateDetailsDocs() {
			Contract.Ensures(Contract.Result<XmlDocsDetails>() != null);
			if (null == DefinitionXmlDocs)
				return new XmlDocsDetails();
			return new XmlDocsDetails(DefinitionXmlDocs);
		}

		private IComplexTextNode CreateSummaryDocs() {
			var xmlDocs = DefinitionXmlDocs;
			if (null == xmlDocs)
				return null;

			return ParsedXmlDocComplexTextNode.Convert(xmlDocs.Summary);
		}

		protected TDefinition Definition { get; private set; }

		public IAssemblySimpleModel ContainingAssembly { get; private set; }

		protected DefinitionXmlDocBase DefinitionXmlDocs { get { return _xmlDocs.Value; } }

		protected bool HasPureXmlDocElement { get { return _detailsDocs.Value.HasPureElement; } }

		public virtual string Title {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<String>()));
				return RegularTypeDisplayNameOverlay.GetDisplayName((MemberReference)Definition);
			}
		}

		public abstract string SubTitle { get; }

		public virtual string ShortName{
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<String>()));
				return RegularTypeDisplayNameOverlay.GetDisplayName((MemberReference)Definition);
			}
		}

		public virtual string FullName{
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<String>()));
				return FullTypeDisplayNameOverlay.GetDisplayName((MemberReference) Definition);
			}
		}

		public string CRef {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<String>()));
				return ContainingAssembly.CRefOverlay.GetCref((MemberReference)Definition);
			}
		}

		public abstract string NamespaceName { get; }

		public ISimpleModelRepository RootRepository {
			get {
				Contract.Ensures(Contract.Result<ISimpleModelRepository>() != null);
				return ContainingAssembly.RootRepository;
			}
		}

		public virtual bool HasFlair {
			get { return FlairTags.Count > 0; }
		}

		public virtual IList<IFlairTag> FlairTags {
			get {
				Contract.Ensures(Contract.Result<IList<IFlairTag>>() != null);
				var results = new List<IFlairTag>();
				if(Definition.IsStatic())
					results.Add(DefaultStaticFlair);
				if(Definition.HasObsoleteAttribute())
					results.Add(DefaultObsoleteTag);
				return results;
			}
		}

		public bool HasSummary { get { return Summary != null; } }

		public IComplexTextNode Summary{
			get{
				Contract.Ensures(
					HasSummary
					? Contract.Result<IComplexTextNode>() != null
					: Contract.Result<IComplexTextNode>() == null);
				return _summaryDocs.Value;
			}
		}

		public bool HasRemarks { get { return Remarks.Count > 0; } }

		public IList<IComplexTextNode> Remarks{
			get{
				Contract.Ensures(Contract.Result<IList<IComplexTextNode>>() != null);
				return _detailsDocs.Value.Remarks;
			}
		}

		public bool HasExamples { get { return Examples.Count > 0; } }

		public IList<IComplexTextNode> Examples{
			get{
				Contract.Ensures(Contract.Result<IList<IComplexTextNode>>() != null);
				return _detailsDocs.Value.Examples;
			}
		}

		public bool HasSeeAlso { get { return SeeAlso.Count > 0; } }

		public IList<IComplexTextNode> SeeAlso{
			get{
				Contract.Ensures(Contract.Result<IList<IComplexTextNode>>() != null);
				return _detailsDocs.Value.SeeAlso;
			}
		}

		public abstract ISimpleModel DeclaringModel { get; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Definition != null);
			Contract.Invariant(ContainingAssembly != null);
		}

	}
}
