using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocException : ICodeDocException
    {

        public CodeDocException(CRefIdentifier cRef) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            ExceptionCRef = cRef;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(ExceptionCRef != null);
        }

        public CRefIdentifier ExceptionCRef { get; private set; }

        public bool HasConditions { get { return Conditions != null && Conditions.Count > 0; } }

        public IList<XmlDocNode> Conditions { get; set; }

        public bool HasEnsures { get { return Ensures != null && Ensures.Count > 0; } }

        public IList<XmlDocNode> Ensures { get; set; }
    }
}
