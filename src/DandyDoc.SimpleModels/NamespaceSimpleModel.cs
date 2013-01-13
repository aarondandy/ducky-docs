using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class NamespaceSimpleModel : INamespaceSimpleModel
	{

		public static IEnumerable<NamespaceSimpleModel> BuildNamespaces(IEnumerable<IAssemblySimpleModel> assemblies, Func<IAssemblySimpleModel, IEnumerable<ITypeSimpleModel>> getTypes, ISimpleModelRepository rootRepository) {
			if(null == assemblies) throw new ArgumentNullException("assemblies");
			if(null == getTypes) throw new ArgumentNullException("getTypes");
			Contract.Ensures(Contract.Result<IEnumerable<NamespaceSimpleModel>>() != null);
			Contract.EndContractBlock();

			var namespaceTypeLookup = assemblies
				.SelectMany(getTypes)
				.ToLookup(t => t.NamespaceName);

			foreach (var set in namespaceTypeLookup){
				var setItems = set.ToArray();
				var setAssemblies = setItems.Select(t => t.ContainingAssembly).Distinct().ToArray();
				yield return new NamespaceSimpleModel(set.Key, setAssemblies, setItems, rootRepository);
			}

		}

		private NamespaceSimpleModel(string namespaceName, IList<IAssemblySimpleModel> assemblies, IList<ITypeSimpleModel> types, ISimpleModelRepository rootRepository) {
			Contract.Requires(namespaceName != null);
			Contract.Requires(assemblies != null);
			Contract.Requires(types != null);
			Contract.Requires(rootRepository != null);
			NamespaceName = namespaceName;
			Assemblies = new ReadOnlyCollection<IAssemblySimpleModel>(assemblies);
			Types = new ReadOnlyCollection<ITypeSimpleModel>(types);
			RootRepository = rootRepository;
		}

		public string NamespaceName { get; private set; }

		public IList<ITypeSimpleModel> Types { get; private set; }

		public IList<IAssemblySimpleModel> Assemblies { get; private set; }

		public ISimpleModelRepository RootRepository { get; private set; }

		public string Title {
			get { return DisplayName; }
		}

		public string SubTitle {
			get { return "Namespace"; }
		}

		public string DisplayName {
			get {
				if (String.IsNullOrEmpty(NamespaceName))
					return "global::";
				return NamespaceName;
			}
		}

		public string CRef {
			get { return "N:" + NamespaceName; }
		}

		public string FullName {
			get { return NamespaceName; }
		}

		IAssemblySimpleModel ISimpleModel.ContainingAssembly {
			get { return Assemblies.FirstOrDefault(); }
		}

		public bool HasFlair {
			get { return false; }
		}

		public IList<IFlairTag> FlairTags {
			get { return new IFlairTag[0]; }
		}
		
		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(NamespaceName != null);
			Contract.Invariant(Types != null);
			Contract.Invariant(Assemblies != null);
			Contract.Invariant(RootRepository != null);
		}

	}
}
