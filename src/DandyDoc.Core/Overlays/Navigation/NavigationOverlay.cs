using System.Collections.ObjectModel;
using System.Threading;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DandyDoc.Core.Overlays.Navigation
{
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
			return Assemblies.ToDictionary(a => a, GenerateNamespaces);
		}

		private ReadOnlyCollection<NavigationOverlayNamespace> GenerateNamespaces(AssemblyDefinition assembly){
			var resultBuilder = new Dictionary<string, List<TypeDefinition>>();
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
				.Select(x => new NavigationOverlayNamespace(x.Key,x.Value))
				.OrderBy(x => x.Namespace)
				.ToArray());
		}

		private ReadOnlyCollection<NavigationOverlayCompositeNamespace> GenerateMergedNamespaces() {
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

		public IDictionary<AssemblyDefinition, IList<NavigationOverlayNamespace>> AssemblyNamespaces {
			get{
				// TODO: better to return/store as a list?
				return _assemblyNamespaces.Value
					.ToDictionary(x => x.Key, x => (IList<NavigationOverlayNamespace>)x.Value);
			}
		}

		public IList<NavigationOverlayCompositeNamespace> Namespaces {
			get { return _mergedNamespaces.Value; }
		} 



	}
}
