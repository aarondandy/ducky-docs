using System;
using System.Collections.Generic;
using DandyDoc.CRef;
using DandyDoc.ExternalVisibility;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code documentation model for a code member.
    /// </summary>
    public interface ICodeDocMember
    {

        /// <summary>
        /// The display title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The display sub title.
        /// </summary>
        string SubTitle { get; }

        /// <summary>
        /// The short name for display purposes.
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// The full name of the member.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// The identifier for this member.
        /// </summary>
        CRefIdentifier CRef { get; }

        /// <summary>
        /// The namespace containing this member.
        /// </summary>
        string NamespaceName { get; }

        /// <summary>
        /// Indicates that this member has a summary.
        /// </summary>
        bool HasSummary { get; }

        /// <summary>
        /// The XML doc summary element for the member.
        /// </summary>
        XmlDocElement Summary { get; }

        /// <summary>
        /// Indicates that there are XML doc summary contents.
        /// </summary>
        bool HasSummaryContents { get; }

        /// <summary>
        /// The XML doc summary content nodes.
        /// </summary>
        IList<XmlDocNode> SummaryContents { get; }

        /// <summary>
        /// Indicates that this member is static.
        /// </summary>
        [Obsolete]
        bool IsStatic { get; }

        /// <summary>
        /// The external visibility of the member.
        /// </summary>
        ExternalVisibilityKind ExternalVisibility { get; }

        /// <summary>
        /// Indicates that this member is obsolete.
        /// </summary>
        [Obsolete]
        bool IsObsolete { get; } 

    }
}
