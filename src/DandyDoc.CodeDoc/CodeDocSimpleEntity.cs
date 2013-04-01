using System;
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

        public XmlDocNode Summary {
            get { return XmlDocs == null ? null : XmlDocs.SummaryElement; }
        }

        public XmlDocMember XmlDocs { get; set; }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(CRef != null);
        }

        public override string ToString() {
            return FullName ?? base.ToString();
        }

    }
}
