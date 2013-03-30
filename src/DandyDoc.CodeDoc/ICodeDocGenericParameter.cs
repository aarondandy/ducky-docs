using System.Collections.Generic;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocGenericParameter
	{

		bool HasTypeConstraints { get; }

		IList<CRefIdentifier> TypeConstraints { get; }

		string Name { get; }

		bool HasSummary { get; }

        XmlDocNode Summary { get; }

		bool IsContravariant { get; }

		bool IsCovariant { get; }

        bool HasReferenceTypeConstraint { get; }

        bool HasNotNullableValueTypeConstraint { get; }

        bool HasDefaultConstructorConstraint { get; }

	}
}
