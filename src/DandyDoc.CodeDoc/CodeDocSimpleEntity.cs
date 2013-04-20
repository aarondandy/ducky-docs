using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocSimpleEntity : ICodeDocEntity
    {

        public CodeDocSimpleEntity(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            CRef = cRef;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(CRef != null);
        }

        public CRefIdentifier CRef { get; private set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string ShortName { get; set; }

        public string FullName { get; set; }

        public string NamespaceName { get; set; }

        public bool HasSummary {
            get {
                var summary = Summary;
                return summary != null && summary.HasChildren;
            }
        }

        public XmlDocElement Summary {
            get { return XmlDocs == null ? null : XmlDocs.SummaryElement; }
        }

        public XmlDocMember XmlDocs { get; set; }

        public bool IsStatic { get; set; }

        public override string ToString() {
            return FullName ?? base.ToString();
        }

        public bool HasSummaryContents {
            get { return XmlDocs != null && XmlDocs.HasSummaryContents; }
        }

        public IList<XmlDocNode> SummaryContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                return XmlDocs != null && XmlDocs.HasSummaryContents
                    ? XmlDocs.SummaryContents
                    : new XmlDocNode[0];
            }
        }

    }
}
