using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;
using DandyDoc.ExternalVisibility;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code documentation simple model for a code member.
    /// </summary>
    public class CodeDocSimpleMember : ICodeDocMember
    {

        /// <summary>
        /// Creates a new code doc model.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocSimpleMember(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            CRef = cRef;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(CRef != null);
        }

        /// <inheritdoc/>
        public CRefIdentifier CRef { get; private set; }

        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public string SubTitle { get; set; }

        /// <inheritdoc/>
        public string ShortName { get; set; }

        /// <inheritdoc/>
        public string FullName { get; set; }

        /// <inheritdoc/>
        public string NamespaceName { get; set; }

        /// <inheritdoc/>
        public ExternalVisibilityKind ExternalVisibility { get; set; }

        [Obsolete]
        public bool HasSummary {
            get {
                var summary = Summary;
                return summary != null && summary.HasChildren;
            }
        }

        [Obsolete]
        public XmlDocElement Summary {
            get { return XmlDocs == null ? null : XmlDocs.SummaryElement; }
        }

        [Obsolete]
        public XmlDocMember XmlDocs { get; set; }

        /// <inheritdoc/>
        public override string ToString() {
            return FullName ?? base.ToString();
        }

        /// <inheritdoc/>
        public bool HasSummaryContents {
            get { return SummaryContents != null && SummaryContents.Count > 0; }
        }

        /// <inheritdoc/>
        public IList<XmlDocNode> SummaryContents { get; set; }

    }
}
