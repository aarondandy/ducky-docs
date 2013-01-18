using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class CrefSimpleMemberPointer : ISimpleMemberPointerModel
	{

		private static string StripCrefTypePrefix(string cRef) {
			if (String.IsNullOrEmpty(cRef))
				return cRef;
			if (cRef.Length >= 2 && cRef[1] == ':')
				return cRef.Substring(2);
			return cRef;
		}

		public CrefSimpleMemberPointer(string cRef)
			: this(cRef, StripCrefTypePrefix(cRef)) { }

		public CrefSimpleMemberPointer(string cRef, string displayName) {
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
