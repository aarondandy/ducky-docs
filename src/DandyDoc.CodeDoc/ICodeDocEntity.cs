using System;
using System.Collections.Generic;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEntity
    {

        string Title { get; }

        string SubTitle { get; }

        string ShortName { get; }

        string FullName { get; }

        CRefIdentifier CRef { get; }

        string NamespaceName { get; }

        bool HasSummary { get; }

        XmlDocElement Summary { get; }

        bool HasSummaryContents { get; }

        IList<XmlDocNode> SummaryContents { get; }

        bool IsStatic { get; }

    }
}
