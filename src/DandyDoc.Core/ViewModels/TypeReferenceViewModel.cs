using System;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.DisplayName;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class TypeReferenceViewModel
	{

		private static readonly DisplayNameOverlay ShortNameOverlay = new DisplayNameOverlay();

		public TypeReferenceViewModel(TypeReference reference) {
			if(null == reference) throw new ArgumentNullException("reference");
			Contract.EndContractBlock();
			Reference = reference;
		}

		public TypeReference Reference { get; private set; }

		public TypeDefinition Definition { get { return Reference.Resolve(); } }

		public string ShortName {
			get { return ShortNameOverlay.GetDisplayName(Reference); }
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Reference);
		}

	}
}
