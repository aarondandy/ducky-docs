using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocGenericParameter
    {

        bool HasTypeConstraints { get; }

        IList<ICodeDocEntity> TypeConstraints { get; }

        string Name { get; }

        bool HasSummary { get; }

        XmlDocElement Summary { get; }

        bool HasSummaryContents { get; }

        IList<XmlDocNode> SummaryContents { get; }

        bool IsContravariant { get; }

        bool IsCovariant { get; }

        bool HasReferenceTypeConstraint { get; }

        bool HasNotNullableValueTypeConstraint { get; }

        bool HasDefaultConstructorConstraint { get; }

        bool HasAnyConstraints { get; }

    }
}
