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
	public class TypeSimpleModel : ITypeSimpleModel
	{

		private static readonly DisplayNameOverlay RegularTypeDisplayNameOverlay = new DisplayNameOverlay{
			ShowTypeNameForMembers = false
		};

		private static readonly DisplayNameOverlay NestedTypeDisplayNameOverlay = new DisplayNameOverlay{
			ShowTypeNameForMembers = true
		};

		private static readonly DisplayNameOverlay FullTypeDisplayNameOverlay = new DisplayNameOverlay {
			ShowTypeNameForMembers = true,
			IncludeNamespaceForTypes = true
		};

		private readonly Lazy<ISimpleModelMembersCollection> _members;
		private readonly Lazy<TypeDefinitionXmlDoc> _xmlDocs;
		private readonly Lazy<IComplexTextNode> _summaryDocs;

		public TypeSimpleModel(TypeDefinition definition, IAssemblySimpleModel assemblyModel){
			if (null == definition) throw new ArgumentNullException("definition");
			if (null == assemblyModel) throw new ArgumentNullException("assemblyModel");
			Contract.EndContractBlock();
			Definition = definition;
			ContainingAssembly = assemblyModel;
			_members = new Lazy<ISimpleModelMembersCollection>(() => ContainingAssembly.GetMembers(this), true);
			_xmlDocs = new Lazy<TypeDefinitionXmlDoc>(() => ContainingAssembly.XmlDocOverlay.GetDocumentation(Definition), true);
			_summaryDocs = new Lazy<IComplexTextNode>(CreateSummaryDocs, true);
		}

		private IComplexTextNode CreateSummaryDocs(){
			var xmlDocs = XmlDocs;
			if (null == xmlDocs)
				return null;

			return ParsedXmlDocComplexTextNode.Convert(xmlDocs.Summary);
		}

		protected ISimpleModelMembersCollection Members{
			get{
				Contract.Ensures(Contract.Result<ISimpleModelMembersCollection>() != null);
				return _members.Value;
			}
		}

		protected TypeDefinition Definition { get; private set; }

		protected TypeDefinitionXmlDoc XmlDocs { get { return _xmlDocs.Value;  } }

		public IAssemblySimpleModel ContainingAssembly { get; private set; }

		public virtual string ShortName {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				return RegularTypeDisplayNameOverlay.GetDisplayName(Definition);
			}
		}

		public virtual string FullName {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				return FullTypeDisplayNameOverlay.GetDisplayName(Definition);
			}
		}

		public virtual string CRef {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				return ContainingAssembly.CrefOverlay.GetCref(Definition);
			}
		}

		public virtual string Title {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				var nameGenerator = Definition.IsNested ? NestedTypeDisplayNameOverlay : RegularTypeDisplayNameOverlay;
				return nameGenerator.GetDisplayName(Definition);
			}
		}

		public virtual string NamespaceName{ get { return Definition.Namespace; } }

		public virtual string SubTitle {
			get {
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				if (Definition.IsEnum)
					return "Enumeration";
				if (Definition.IsValueType)
					return "Structure";
				if (Definition.IsInterface)
					return "Interface";
				if (Definition.IsDelegateType())
					return "Delegate";
				return "Class";
			}
		}

		public ISimpleModelRepository RootRepository {
			get{
				Contract.Ensures(Contract.Result<ISimpleModelRepository>() != null);
				return ContainingAssembly.RootRepository;
			}
		}

		public IList<ITypeSimpleModel> NestedTypes { get { return Members.Types; } }

		public IList<IDelegateSimpleModel> NestedDelegates { get { return Members.Delegates; } }

		public bool HasFlair {
			get { return FlairTags.Count > 0; }
		}

		public IList<IFlairTag> FlairTags {
			get { throw new NotImplementedException(); }
		}

		public bool HasSummary { get { return Summary != null; } }
		public IComplexTextNode Summary { get { return _summaryDocs.Value; } }

		public bool HasRemarks { get { return Remarks.Count > 0; } }
		public IList<IComplexTextNode> Remarks { get { throw new NotImplementedException(); } }

		public bool HasExamples { get { return Examples.Count > 0; } }
		public IList<IComplexTextNode> Examples { get { throw new NotImplementedException(); } }

		public bool HasSeeAlso { get { return SeeAlso.Count > 0; } }
		public IList<IComplexTextNode> SeeAlso { get { throw new NotImplementedException(); } }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Definition != null);
			Contract.Invariant(ContainingAssembly != null);
		}

	}
}
