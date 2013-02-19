using System.Collections.Generic;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocInvokable
	{

		/*
		TODO: Is there a way to handle these outside of implementations of this class? 

		bool AllReferenceParamsAndReturnNotNull { get; }

		bool EnsuresResultNotNull { get; }

		bool EnsuresResultNotNullOrEmpty { get; }

		bool RequiresParameterNotNull(string parameterName);

		bool RequiresParameterNotNullOrEmpty(string parameterName);

		bool IsPure { get; }

		bool CanReturnNull { get; }
		*/

		bool HasParameters { get; }

		IList<ICodeDocParameter> Parameters { get; }

		bool HasReturn { get; }

		ICodeDocParameter Return { get; }

		bool HasExceptions { get; }

		IList<ICodeDocException> Exceptions { get; }

		bool HasEnsures { get; }

		IList<ICodeDocContractCondition> Ensures { get; }

		bool HasRequires { get; }

		IList<ICodeDocContractCondition> Requires { get; }

	}
}
