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
			return StringComparer.OrdinalIgnoreCase.Compare(a.DisplayName, b.DisplayName);
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
			return StringComparer.OrdinalIgnoreCase.Compare(a.DisplayName, b.DisplayName);
		}

		// ------------ Public repository access

		public ISimpleModel GetModelFromCref(string cref){
			if(String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid CRef", "cref");
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


	}
}
