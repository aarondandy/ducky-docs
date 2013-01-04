using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.Overlays.Navigation
{
	public class NavigationOverlayNamespace
	{

		internal NavigationOverlayNamespace(string ns, IList<TypeDefinition> types){
			Contract.Requires(null != ns);
			Contract.Requires(null != types);
			Contract.Requires(Contract.ForAll(types, type => null != type));
			Namespace = ns;
			Types = new ReadOnlyCollection<TypeDefinition>(types);
		}

		public string Namespace { get; private set; }

		public ReadOnlyCollection<TypeDefinition> Types { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != Namespace);
			Contract.Invariant(null != Types);
			Contract.Invariant(Contract.ForAll(Types, type => null != type));
		}

	}
}
