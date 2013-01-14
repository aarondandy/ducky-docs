using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class CrefSimpleMemberPointer : ISimpleMemberPointerModel
	{

		public CrefSimpleMemberPointer(string displayName, string cRef){
			if(String.IsNullOrEmpty(displayName)) throw new ArgumentException("Display name is required.");
			Contract.EndContractBlock();
			MemberDisplayName = displayName;
			CRef = cRef ?? String.Empty;
		}

		public string MemberDisplayName { get; private set; }

		public string CRef { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(!String.IsNullOrEmpty(MemberDisplayName));
			Contract.Invariant(CRef != null);
		}

	}
}
