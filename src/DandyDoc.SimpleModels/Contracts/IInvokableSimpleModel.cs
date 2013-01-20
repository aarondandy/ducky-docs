using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IInvokableSimpleModel
	{

		bool AllReferenceParamsAndReturnNotNull { get; }

		bool EnsuresResultNotNull { get; }

		bool EnsuresResultNotNullOrEmpty { get; }

		bool RequiresParameterNotNull(string parameterName);

		bool RequiresParameterNotNullOrEmpty(string parameterName);

		bool HasParameters { get; }

		IList<IParameterSimpleModel> Parameters { get; }

		bool HasReturn { get; }

		IParameterSimpleModel Return { get; }

		bool HasExceptions { get; }

		IList<IExceptionSimpleModel> Exceptions { get; }

		bool IsPure { get; }

		bool CanReturnNull { get; }

	}
}
