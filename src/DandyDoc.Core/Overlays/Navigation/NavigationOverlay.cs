﻿using System.Collections.ObjectModel;
using System.Threading;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DandyDoc.Overlays.Navigation
{
	[Obsolete("This should be handled by the view.")]
	public class NavigationOverlay
	{

		private readonly Lazy<Dictionary<AssemblyDefinition, ReadOnlyCollection<NavigationOverlayNamespace>>> _assemblyNamespaces;
		private readonly Lazy<ReadOnlyCollection<NavigationOverlayCompositeNamespace>> _mergedNamespaces;

		public NavigationOverlay(AssemblyDefinitionCollection assemblyDefinitionCollection) {
			if(null == assemblyDefinitionCollection) throw new ArgumentNullException("assemblyDefinitionCollection");
			Contract.EndContractBlock();
			Assemblies = assemblyDefinitionCollection;
			_assemblyNamespaces = new Lazy<Dictionary<AssemblyDefinition, ReadOnlyCollection<NavigationOverlayNamespace>>>(
				GenerateAssemblyNamespaceLookup, LazyThreadSafetyMode.ExecutionAndPublication);
			_mergedNamespaces = new Lazy<ReadOnlyCollection<NavigationOverlayCompositeNamespace>>(
				GenerateMergedNamespaces, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		private Dictionary<AssemblyDefinition, ReadOnlyCollection<NavigationOverlayNamespace>> GenerateAssemblyNamespaceLookup(){
			Contract.Ensures(Contract.Result<Dictionary<AssemblyDefinition, ReadOnlyCollection<NavigationOverlayNamespace>>>() != null);
			return Assemblies.ToDictionary(a => a, GenerateNamespaces);
		}

		private ReadOnlyCollection<NavigationOverlayNamespace> GenerateNamespaces(AssemblyDefinition assembly){
			Contract.Requires(null != assembly);
			Contract.Ensures(Contract.Result<ReadOnlyCollection<NavigationOverlayNamespace>>() != null);
			var resultBuilder = new Dictionary<string, List<TypeDefinition>>();
			Contract.Assume(null != assembly.Modules);
			foreach (var type in assembly.Modules.SelectMany(m => m.Types)){
				var ns = type.Namespace;
				List<TypeDefinition> tdList;
				if (!resultBuilder.TryGetValue(ns, out tdList)){
					tdList = new List<TypeDefinition>();
					resultBuilder.Add(ns, tdList);
				}
				tdList.Add(type);
			}
			return Array.AsReadOnly(
				resultBuilder
				.Select(x => new NavigationOverlayNamespace(x.Key,x.Value, assembly))
				.OrderBy(x => x.Namespace)
				.ToArray());
		}

		private ReadOnlyCollection<NavigationOverlayCompositeNamespace> GenerateMergedNamespaces() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<NavigationOverlayCompositeNamespace>>() != null);
			var resultBuilder = new Dictionary<string, List<NavigationOverlayNamespace>>();
			foreach (var assemblyGroup in _assemblyNamespaces.Value){
				foreach (var ns in assemblyGroup.Value){
					var nsName = ns.Namespace;
					List<NavigationOverlayNamespace> nsList;
					if (!resultBuilder.TryGetValue(nsName, out nsList)){
						nsList = new List<NavigationOverlayNamespace>(1);
						resultBuilder.Add(nsName, nsList);
					}
					nsList.Add(ns);
				}
			}

			return Array.AsReadOnly(
				resultBuilder
				.Select(x => new NavigationOverlayCompositeNamespace(x.Key, x.Value))
				.OrderBy(x => x.Namespace)
				.ToArray());
		}

		public AssemblyDefinitionCollection Assemblies { get; private set; }

		public IList<NavigationOverlayCompositeNamespace> Namespaces {
			get {
				Contract.Ensures(Contract.Result<IList<NavigationOverlayCompositeNamespace>>() != null);
				return _mergedNamespaces.Value;
			}
		} 

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != Assemblies);
		}

	}
}
