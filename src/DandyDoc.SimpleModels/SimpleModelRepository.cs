using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class SimpleModelRepository : ISimpleModelRepository
	{

		

		//private readonly object _repositoryMutex = new object();
		private readonly Lazy<ReadOnlyCollection<IAssemblySimpleModel>> _assemblies;
		private readonly Lazy<ReadOnlyCollection<INamespaceSimpleModel>> _namespaces;

		protected AssemblyDefinitionCollection AssemblyDefinitions { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; } 

		public SimpleModelRepository(AssemblyDefinitionCollection assemblyDefinitions)
			: this(assemblyDefinitions, new XmlDocOverlay(new CrefOverlay(assemblyDefinitions)), null)
		{
			Contract.Requires(assemblyDefinitions != null);
		}

		public SimpleModelRepository(AssemblyDefinitionCollection assemblyDefinitions, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay) {
			if(null == assemblyDefinitions) throw new ArgumentNullException("assemblyDefinitions");
			if(null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();

			AssemblyDefinitions = assemblyDefinitions;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
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
			return Array.AsReadOnly(assemblyModels);
		}

		protected virtual ReadOnlyCollection<INamespaceSimpleModel> BuildNamespaces() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<INamespaceSimpleModel>>() != null);
			throw new NotImplementedException();
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(XmlDocOverlay != null);
			Contract.Invariant(CrefOverlay != null);
		}

		// ------------ Public repository access

		public IList<IAssemblySimpleModel> Assemblies {
			get {
				Contract.Ensures(Contract.Result<IList<IAssemblySimpleModel>>() != null);
				//lock (_repositoryMutex) {
					return _assemblies.Value;
				//}
			}
		}

		public IList<INamespaceSimpleModel> Namespaces {
			get {
				Contract.Ensures(Contract.Result<IList<INamespaceSimpleModel>>() != null);
				//lock (_repositoryMutex) {
					return _namespaces.Value;
				//}
			}
		}


	}
}
