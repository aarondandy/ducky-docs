using System.Collections.Generic;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public interface IParameterizedDefinitionViewModel
	{

		bool AllResultsAndParamsNotNull { get; }

		bool HasReturn { get; }

		TypeReference ReturnType { get; }

		bool HasParameters { get; }

		IList<ParameterDefinition> Parameters { get; }

		bool EnsuresResultNotNull { get; }

		bool EnsuresResultNotNullOrEmpty { get; }

		bool RequiresParameterNotNull(string parameterName);

		bool RequiresParameterNotNullOrEmpty(string parameterName);

		bool CanReturnNull { get; }

	}
}
