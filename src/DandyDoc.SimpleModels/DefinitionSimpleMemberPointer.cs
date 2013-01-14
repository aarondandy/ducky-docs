using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.SimpleModels.Contracts;
using System;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class DefinitionSimpleMemberPointer : ISimpleMemberPointerModel
	{

		public DefinitionSimpleMemberPointer(string displayName, IMemberDefinition definition){
			if (String.IsNullOrEmpty(displayName)) throw new ArgumentException("Display name is required.");
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			MemberDisplayName = displayName;
			Definition = definition;
		}

		public string MemberDisplayName { get; private set; }

		public IMemberDefinition Definition { get; private set; }

		public string CRef {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return CrefOverlay.GetDefaultCref(Definition);
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(!String.IsNullOrEmpty(MemberDisplayName));
			Contract.Invariant(Definition != null);
		}

	}
}
