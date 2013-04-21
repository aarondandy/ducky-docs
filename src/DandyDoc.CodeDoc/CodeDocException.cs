using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocException : ICodeDocException
    {

        public CodeDocException(ICodeDocEntity exceptionType) {
            if (exceptionType == null) throw new ArgumentNullException("exceptionType");
            Contract.EndContractBlock();
            ExceptionType = exceptionType;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(ExceptionType != null);
        }

        public ICodeDocEntity ExceptionType { get; private set; }

        public bool HasConditions { get { return Conditions != null && Conditions.Count > 0; } }

        public IList<XmlDocNode> Conditions { get; set; }

        public bool HasEnsures { get { return Ensures != null && Ensures.Count > 0; } }

        public IList<XmlDocNode> Ensures { get; set; }
    }
}
