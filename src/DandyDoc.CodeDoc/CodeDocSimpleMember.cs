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
        private CRefIdentifier _cRef;

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
            Contract.Invariant(_cRef != null);
            Contract.Invariant(CRef != null);
        }

        /// <inheritdoc/>
        public CRefIdentifier CRef {
            get { return _cRef; }
            set {
                if(value == null) throw new ArgumentNullException("value");
                Contract.EndContractBlock();
                _cRef = value;
            }
        }

        /// <inheritdoc/>
        public Uri Uri { get; set; }

        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public string SubTitle { get; set; }

        /// <inheritdoc/>
        public string ShortName { get; set; }

        /// <inheritdoc/>
        public string FullName { get; set; }

        /// <inheritdoc/>
        [Obsolete("This is redundant with Namesapce.Something")]
        public string NamespaceName { get; set; }

        /// <summary>
        /// Gets the namespace for this model.
        /// </summary>
        public ICodeDocMember Namespace { get; set; }

        /// <inheritdoc/>
        public ExternalVisibilityKind ExternalVisibility { get; set; }

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
