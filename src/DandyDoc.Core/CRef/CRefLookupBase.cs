using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DandyDoc.CRef
{
	public abstract class CRefLookupBase<TAssembly, TMember>
	{

		private readonly ReadOnlyCollection<TAssembly> _assemblies;

		protected CRefLookupBase(IEnumerable<TAssembly> assemblies) {
			if (assemblies == null) throw new ArgumentNullException("assemblies");
			Contract.EndContractBlock();
			_assemblies = new ReadOnlyCollection<TAssembly>(assemblies.ToArray());
		}

		public ReadOnlyCollection<TAssembly> Assemblies {
			get {
				Contract.Ensures(Contract.Result<IList<TAssembly>>() != null);
				return _assemblies;
			}
		}

		public abstract TMember GetMember(string cRef);

		public abstract TMember GetMember(CRefIdentifier cRef);

	}
}
