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

        bool HasGenericTypeSummaryContents(string typeParameterName);

        IEnumerable<XmlDocNode> GetGenericTypeSummaryContents(string typeParameterName); 

        bool? RequiresParameterNotEverNull(string parameterName);

        bool? EnsuresResultNotEverNull { get; }

        bool HasExceptions { get; }

        IEnumerable<XmlDocRefElement> GetExceptions();

        bool HasEnsures { get; }

        IEnumerable<XmlDocContractElement> GetEnsures();

        bool HasRequires { get; }

        IEnumerable<XmlDocContractElement> GetRequires();

        bool HasInvariants { get; }

        IEnumerable<XmlDocContractElement> GetInvariants();

    }
}
