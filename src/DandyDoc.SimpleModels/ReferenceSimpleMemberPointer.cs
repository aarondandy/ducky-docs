using System;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class ReferenceSimpleMemberPointer : ISimpleMemberPointerModel
	{

		public ReferenceSimpleMemberPointer(string displayName, MemberReference reference) {
			if (String.IsNullOrEmpty(displayName)) throw new ArgumentException("Display name is required.");
			if (null == reference) throw new ArgumentNullException("reference");
			Contract.EndContractBlock();
			MemberDisplayName = displayName;
			Reference = reference;
		}

		public string MemberDisplayName { get; private set; }

		public MemberReference Reference { get; private set; }

		public string CRef {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return CrefOverlay.GetDefaultCref(Reference);
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(!String.IsNullOrEmpty(MemberDisplayName));
			Contract.Invariant(Reference != null);
		}

	}
}
