using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IMethodSimpleModel : ISimpleModel
	{

		bool HasGenericParameters { get; }

		IList<IGenericParameterSimpleModel> GenericParameters { get; }

		bool HasParameters { get; }

		IList<IParameterSimpleModel> Parameters { get; }

		bool HasReturn { get; }

		IParameterSimpleModel Return { get; }

		bool HasExceptions { get; }

		IList<IExceptionSimpleModel> Exceptions { get; }

		bool HasEnsures { get; }

		IList<IContractConditionSimpleModel> Ensures { get; }

		bool HasRequires { get; }

		IList<IContractConditionSimpleModel> Requires { get; }

	}
}
