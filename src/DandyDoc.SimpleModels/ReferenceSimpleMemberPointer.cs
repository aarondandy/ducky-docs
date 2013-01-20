using System;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class ReferenceSimpleMemberPointer : ISimpleMemberPointerModel
	{

		public ReferenceSimpleMemberPointer(MemberReference reference, string description) {
			if (null == reference) throw new ArgumentNullException("reference");
			Contract.EndContractBlock();
			Description = String.IsNullOrEmpty(description)
				? new StandardComplexText(reference.FullName)
				: new StandardComplexText(description);
			Reference = reference;
		}

		public ReferenceSimpleMemberPointer(MemberReference reference, IComplexTextNode description = null) {
			if (null == reference) throw new ArgumentNullException("reference");
			Contract.EndContractBlock();
			Description = description ?? new StandardComplexText(reference.FullName);
			Reference = reference;
		}

		public IComplexTextNode Description { get; private set; }

		public MemberReference Reference { get; private set; }

		public string CRef {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return CRefOverlay.GetDefaultCref(Reference);
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Description != null);
			Contract.Invariant(Reference != null);
		}

	}
}
