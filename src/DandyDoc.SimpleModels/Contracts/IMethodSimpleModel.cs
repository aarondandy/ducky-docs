using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IMethodSimpleModel : ISimpleModel, IInvokableSimpleModel
	{

		bool HasGenericParameters { get; }

		IList<IGenericParameterSimpleModel> GenericParameters { get; }

		bool HasEnsures { get; }

		IList<IContractConditionSimpleModel> Ensures { get; }

		bool HasRequires { get; }

		IList<IContractConditionSimpleModel> Requires { get; }

	}
}
