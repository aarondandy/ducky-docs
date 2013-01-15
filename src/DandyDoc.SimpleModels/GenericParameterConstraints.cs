using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class MemberPointerGenericConstraint : IGenericParameterConstraint
	{

		public MemberPointerGenericConstraint(ISimpleMemberPointerModel member){
			if(null == member) throw new ArgumentNullException("member");
			Contract.EndContractBlock();
			Member = member;
		}

		public ISimpleMemberPointerModel Member { get; private set; }

		public string DisplayName {
			get { return Member.MemberDisplayName; }
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Member != null);
		}

	}

	public class DefaultConstructorGenericConstraint : IGenericParameterConstraint
	{
		public string DisplayName { get { return "Default Constructor"; } }
	}

	public class ValueTypeGenericConstraint : IGenericParameterConstraint
	{
		public string DisplayName { get { return "Value Type"; } }
	}

	public class ReferenceTypeGenericConstraint : IGenericParameterConstraint
	{
		public string DisplayName { get { return "Reference Type"; } }
	}

}
