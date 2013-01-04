using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc.Overlays.Navigation
{
	public class NavigationOverlayCompositeNamespace
	{

		internal NavigationOverlayCompositeNamespace(string ns, IList<NavigationOverlayNamespace> components) {
			Contract.Requires(null != ns);
			Contract.Requires(null != components);
			Contract.Requires(Contract.ForAll(components, component => component.Namespace == ns));
			Namespace = ns;
			Components = new ReadOnlyCollection<NavigationOverlayNamespace>(components);
			Types = new ReadOnlyCollection<TypeDefinition>(components.SelectMany(x => x.Types).ToArray());
		}

		public string Namespace { get; private set; }

		public ReadOnlyCollection<NavigationOverlayNamespace> Components { get; private set; }

		public ReadOnlyCollection<TypeDefinition> Types { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != Namespace);
			Contract.Invariant(null != Types);
			Contract.Invariant(null != Components);
			Contract.Invariant(Contract.ForAll(Components, component => component.Namespace == Namespace));
		}


	}
}
