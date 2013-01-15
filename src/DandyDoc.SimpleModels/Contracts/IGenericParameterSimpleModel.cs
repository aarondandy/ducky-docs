using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IGenericParameterSimpleModel
	{

		bool HasConstraints { get; }

		IList<IGenericParameterConstraint> Constraints { get; }

		string DisplayName { get; }

		bool HasSummary { get; }

		IComplexTextNode Summary { get; }

		bool IsContravariant { get; }

		bool IsCovariant { get; }

	}
}
