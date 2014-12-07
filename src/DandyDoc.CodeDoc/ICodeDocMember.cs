using System;
using System.Collections.Generic;
using DuckyDocs.CRef;
using DuckyDocs.ExternalVisibility;
using DuckyDocs.XmlDoc;

namespace DuckyDocs.CodeDoc
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
        /// A URI that can be used to locate the member.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// The namespace containing this member.
        /// </summary>
        string NamespaceName { get; }

        /// <summary>
        /// Indicates that there are XML doc summary contents.
        /// </summary>
        bool HasSummaryContents { get; }

        /// <summary>
        /// The XML doc summary content nodes.
        /// </summary>
        IList<XmlDocNode> SummaryContents { get; }

        /// <summary>
        /// The external visibility of the member.
        /// </summary>
        ExternalVisibilityKind ExternalVisibility { get; }

    }
}
