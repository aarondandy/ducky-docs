using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class SimpleModelRepository : ISimpleModelRepository
	{

		private readonly Lazy<ReadOnlyCollection<IAssemblySimpleModel>> _assemblies;
		private readonly Lazy<ReadOnlyCollection<INamespaceSimpleModel>> _namespaces;

		protected AssemblyDefinitionCollection AssemblyDefinitions { get; private set; }

		public SimpleModelRepository(AssemblyDefinitionCollection assemblyDefinitions) {
			if(null == assemblyDefinitions) throw new ArgumentNullException("assemblyDefinitions");
			Contract.EndContractBlock();

			AssemblyDefinitions = assemblyDefinitions;
			_assemblies = new Lazy<ReadOnlyCollection<IAssemblySimpleModel>>(BuildAssemblies, true);
			_namespaces = new Lazy<ReadOnlyCollection<INamespaceSimpleModel>>(BuildNamespaces, true);
		}

		protected virtual ReadOnlyCollection<IAssemblySimpleModel> BuildAssemblies() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<IAssemblySimpleModel>>() != null);
			var assemblyModels = new IAssemblySimpleModel[AssemblyDefinitions.Count];
			for (int i = 0; i < assemblyModels.Length; i++) {
				Contract.Assume(null != AssemblyDefinitions[i]);
				assemblyModels[i] = new AssemblySimpleModel(AssemblyDefinitions[i], this);
			}
			Array.Sort(assemblyModels, AssemblyModelCompare);
			return Array.AsReadOnly(assemblyModels);
		}

		protected virtual int AssemblyModelCompare(IAssemblySimpleModel a, IAssemblySimpleModel b) {
			if (a == null)
				return b == null ? 0 : -1;
			if (b == null)
				return 1;
			return StringComparer.OrdinalIgnoreCase.Compare(a.ShortName, b.ShortName);
		}

		protected virtual ReadOnlyCollection<INamespaceSimpleModel> BuildNamespaces() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<INamespaceSimpleModel>>() != null);
			Contract.Assume(_assemblies.Value != null);
			var namespaceModels = NamespaceSimpleModel.BuildNamespaces(_assemblies.Value, a => a.RootTypes, this).Cast<INamespaceSimpleModel>().ToArray();
			Array.Sort(namespaceModels, NamespaceModelCompare);
			return Array.AsReadOnly(namespaceModels);
		}

		protected virtual int NamespaceModelCompare(INamespaceSimpleModel a, INamespaceSimpleModel b) {
			if (a == null)
				return b == null ? 0 : -1;
			if (b == null)
				return 1;
			return StringComparer.OrdinalIgnoreCase.Compare(a.ShortName, b.ShortName);
		}

		// ------------ Public repository access

		public ISimpleModel GetModelFromCref(string cref){
			if(String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid CRef", "cref");

			if (cref.StartsWith("N:")){
				var namespaceName = cref.Substring(2);
				return Namespaces.FirstOrDefault(n => n.NamespaceName == namespaceName);
			}

			return Assemblies.Select(a => a.GetModelFromCref(cref)).FirstOrDefault(m => null != m);
		}

		public IList<IAssemblySimpleModel> Assemblies {
			get {
				Contract.Ensures(Contract.Result<IList<IAssemblySimpleModel>>() != null);
				return _assemblies.Value;
			}
		}

		public IList<INamespaceSimpleModel> Namespaces {
			get {
				Contract.Ensures(Contract.Result<IList<INamespaceSimpleModel>>() != null);
				return _namespaces.Value;
			}
		}

		public string Title {
			get { return "All Namespaces"; }
		}

		public string SubTitle {
			get { return String.Empty; }
		}

		public string ShortName {
			get { return Title; }
		}

		public string FullName {
			get { return Title; }
		}

		public string CRef {
			get { return String.Empty; }
		}

		public string NamespaceName {
			get { return String.Empty; }
		}

		public IAssemblySimpleModel ContainingAssembly {
			get { return null; }
		}

		public ISimpleModelRepository RootRepository {
			get { return this; }
		}

		public bool HasFlair {
			get { return false; }
		}

		public IList<IFlairTag> FlairTags {
			get { return new IFlairTag[0]; }
		}

		public bool HasSummary {
			get { return false; }
		}

		public IComplexTextNode Summary {
			get { return null; }
		}

		public bool HasRemarks {
			get { return false; }
		}

		public IList<IComplexTextNode> Remarks {
			get { return new IComplexTextNode[0]; }
		}

		public bool HasExamples {
			get { return false; }
		}

		public IList<IComplexTextNode> Examples {
			get { return new IComplexTextNode[0]; }
		}

		public bool HasSeeAlso {
			get { return false; }
		}

		public IList<IComplexTextNode> SeeAlso {
			get { return new IComplexTextNode[0]; }
		}
	}
}
