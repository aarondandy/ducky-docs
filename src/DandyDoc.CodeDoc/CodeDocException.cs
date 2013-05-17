using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A code doc exception model.
    /// </summary>
    public class CodeDocException
    {

        /// <summary>
        /// Creates a new exception for the given exception type.
        /// </summary>
        /// <param name="exceptionType">The exception type model for this exception.</param>
        public CodeDocException(ICodeDocMember exceptionType) {
            if (exceptionType == null) throw new ArgumentNullException("exceptionType");
            Contract.EndContractBlock();
            ExceptionType = exceptionType;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(ExceptionType != null);
        }

        /// <summary>
        /// The exception type model.
        /// </summary>
        public ICodeDocMember ExceptionType { get; private set; }

        /// <summary>
        /// Indicates that the exception has conditions.
        /// </summary>
        public bool HasConditions { get { return Conditions != null && Conditions.Count > 0; } }

        /// <summary>
        /// Gets the exception conditions.
        /// </summary>
        public IList<XmlDocNode> Conditions { get; set; }

        /// <summary>
        /// Indicates that there are ensures contracts associated with this exception.
        /// </summary>
        public bool HasEnsures { get { return Ensures != null && Ensures.Count > 0; } }

        /// <summary>
        /// Gets the ensures contracts associated with this exception.
        /// </summary>
        public IList<XmlDocNode> Ensures { get; set; }
    }
}
