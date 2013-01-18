using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class ContractConditionSimpleModel : IContractConditionSimpleModel
	{

		public ContractConditionSimpleModel(IComplexTextNode description, ISimpleMemberPointerModel exception) {
			if(null == description) throw new ArgumentNullException("description");
			Contract.EndContractBlock();
			Exception = exception;
			Description = description;
		}

		public bool HasException { get { return Exception != null; } }

		public ISimpleMemberPointerModel Exception { get; private set; }

		public IComplexTextNode Description { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Description != null);
		}

	}
}
