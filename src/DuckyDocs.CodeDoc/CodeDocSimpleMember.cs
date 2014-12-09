using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using DuckyDocs.CRef;
using DuckyDocs.ExternalVisibility;
using DuckyDocs.XmlDoc;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// A code documentation simple model for a code member.
    /// </summary>
    [DataContract]
    public class CodeDocSimpleMember : ICodeDocMember
    {
        private CRefIdentifier _cRef;

        /// <summary>
        /// Creates a new code doc model.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocSimpleMember(CRefIdentifier cRef) {
            CRef = cRef;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_cRef != null);
        }

        /// <inheritdoc/>
        public override string ToString() {
            return FullName ?? base.ToString();
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public CRefIdentifier CRef {
            get {
                Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
                return _cRef;
            }
            set {
                _cRef = value ?? CRefIdentifier.Invalid;
            }
        }

        /// <summary>
        /// Exposes the code reference (<see cref="CRef"/>) as a text property.
        /// </summary>
        [DataMember]
        public string CRefText {
            get { return _cRef.FullCRef; }
            set { CRef = new CRefIdentifier(value); }
        }

        /// <inheritdoc/>
        [DataMember]
        public Uri Uri { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string Title { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string SubTitle { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string ShortName { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string FullName { get; set; }

        /// <inheritdoc/>
        [Obsolete("This is redundant with Namesapce.FullName")]
        [DataMember]
        public string NamespaceName { get; set; }

        /// <summary>
        /// Gets the namespace for this model.
        /// </summary>
        [DataMember]
        public ICodeDocMember Namespace { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public ExternalVisibilityKind ExternalVisibility { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool HasSummaryContents {
            get { return SummaryContents != null && SummaryContents.Count > 0; }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IList<XmlDocNode> SummaryContents { get; set; }

    }
}
