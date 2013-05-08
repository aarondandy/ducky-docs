using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocParameter
    {

        string Name { get; }

        ICodeDocEntity ParameterType { get; }

        bool HasSummary { get; }

        XmlDocElement Summary { get; }

        bool HasSummaryContents { get; }

        IList<XmlDocNode> SummaryContents { get; }

        bool IsOut { get; }

        bool IsByRef { get; }

        bool? NullRestricted { get; }

        bool? IsReferenceType { get; }

    }
}
