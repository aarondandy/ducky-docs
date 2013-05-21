using System.Collections.Generic;
using DandyDoc.ExternalVisibility;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocMemberDataProvider
    {
        bool HasSummaryContents { get; }

        IEnumerable<XmlDocNode> GetSummaryContents();

        bool HasExamples { get; }

        IEnumerable<XmlDocElement> GetExamples();

        bool HasPermissions { get; }

        IEnumerable<XmlDocRefElement> GetPermissions();

        bool HasValueDescriptionContents { get; }

        IEnumerable<XmlDocNode> GeValueDescriptionContents();

        bool HasRemarks { get; }

        IEnumerable<XmlDocElement> GetRemarks();

        bool HasSeeAlsos { get; }

        IEnumerable<XmlDocRefElement> GetSeeAlsos();

        bool? IsPure { get; }

        ExternalVisibilityKind? ExternalVisibility { get; }

        bool? IsStatic { get; }

        bool? IsObsolete { get; }

        bool HasParameterSummaryContents(string parameterName);

        IEnumerable<XmlDocNode> GetParameterSummaryContents(string parameterName);

        bool HasReturnSummaryContents { get; }

        IEnumerable<XmlDocNode> GetReturnSummaryContents();

        bool? RequiresParameterNotEverNull(string parameterName);

        bool? EnsuresResultNotEverNull { get; }

    }
}
