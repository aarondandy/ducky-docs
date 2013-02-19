using System.Collections.Generic;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocGenericParameter
	{

		bool HasConstraints { get; }

		IList<ICodeDocGenericParameterConstraint> Constraints { get; }

		string Name { get; }

		bool HasSummary { get; }

		bool IsContravariant { get; }

		bool IsCovariant { get; }

	}
}
