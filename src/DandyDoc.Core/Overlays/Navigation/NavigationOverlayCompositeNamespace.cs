using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.Navigation
{
	public class NavigationOverlayCompositeNamespace
	{

		internal NavigationOverlayCompositeNamespace(string ns, IList<NavigationOverlayNamespace> components) {
			if (null == ns) throw new ArgumentNullException("ns");
			if (null == components) throw new ArgumentNullException("components");
			Contract.EndContractBlock();
			foreach(var component in components)
				if(component.Namespace != ns)
					throw new ArgumentException("Component namespace does not match the given namespace.");

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
		}


	}
}
