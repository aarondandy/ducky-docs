using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.ComplexText;
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

		public IComplexTextNode DisplayName {
			get { return Member.Description; }
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Member != null);
		}

	}

	public class DefaultConstructorGenericConstraint : IGenericParameterConstraint
	{
		public IComplexTextNode DisplayName { get { return new StandardComplexText("Default Constructor"); } }
	}

	public class ValueTypeGenericConstraint : IGenericParameterConstraint
	{
		public IComplexTextNode DisplayName { get { return new StandardComplexText("Value Type"); } }
	}

	public class ReferenceTypeGenericConstraint : IGenericParameterConstraint
	{
		public IComplexTextNode DisplayName { get { return new StandardComplexText("Reference Type"); } }
	}

}
