using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using System.Collections.ObjectModel;

namespace DandyDoc.SimpleModels
{
	public class ExceptionSimpleModel : IExceptionSimpleModel
	{

		public ExceptionSimpleModel(ISimpleMemberPointerModel exceptionType, IList<IComplexTextNode> conditions = null, IList<IComplexTextNode> ensures = null){
			if(null == exceptionType) throw new ArgumentNullException("exceptionType");
			Contract.EndContractBlock();
			ExceptionType = exceptionType;
			Conditions = new ReadOnlyCollection<IComplexTextNode>(conditions ?? new IComplexTextNode[0]);
			Ensures = new ReadOnlyCollection<IComplexTextNode>(ensures ?? new IComplexTextNode[0]);
		}

		public ISimpleMemberPointerModel ExceptionType { get; private set; }

		public bool HasConditions { get { return Conditions.Count > 0; } }

		public IList<IComplexTextNode> Conditions { get; private set; }

		public bool HasEnsures { get { return Ensures.Count > 0; } }

		public IList<IComplexTextNode> Ensures { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(ExceptionType != null);
			Contract.Invariant(Conditions != null);
			Contract.Invariant(Ensures != null);
		}

	}
}
