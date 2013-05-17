using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code doc model for a delegate member.
    /// </summary>
    public class CodeDocDelegate : CodeDocType, ICodeDocInvokable
    {
        /// <summary>
        /// Creates a new model for a delegate member.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocDelegate(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        /// <inheritdoc/>
        public bool HasParameters { get { return Parameters != null && Parameters.Count > 0; } }

        /// <inheritdoc/>
        public IList<CodeDocParameter> Parameters { get; set; }

        /// <inheritdoc/>
        public bool HasReturn { get { return Return != null; } }

        /// <inheritdoc/>
        public CodeDocParameter Return { get; set; }

        /// <inheritdoc/>
        public bool HasExceptions { get { return Exceptions != null && Exceptions.Count > 0; } }

        /// <inheritdoc/>
        public IList<CodeDocException> Exceptions { get; set; }

        /// <inheritdoc/>
        public bool HasEnsures { get { return Ensures != null && Ensures.Count > 0; } }

        /// <inheritdoc/>
        public IList<XmlDocContractElement> Ensures { get; set; }

        /// <inheritdoc/>
        public bool HasNormalTerminationEnsures {
            get {
                return HasEnsures
                    && NormalTerminationEnsures.Any();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocContractElement> NormalTerminationEnsures {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<XmlDocContractElement>>() != null);
                return HasEnsures
                    ? Ensures.Where(x => "ENSURES".Equals(x.Name, StringComparison.OrdinalIgnoreCase))
                    : Enumerable.Empty<XmlDocContractElement>();
            }
        }

        /// <inheritdoc/>
        public bool HasRequires { get { return Requires != null && Requires.Count > 0; } }

        /// <inheritdoc/>
        public IList<XmlDocContractElement> Requires { get; set; }

        /// <inheritdoc/>
        public bool IsPure { get; set; }

        /// <inheritdoc/>
        public bool IsExtensionMethod { get { return false; } }

        /// <inheritdoc/>
        public bool IsOperatorOverload { get { return false; } }

        /// <inheritdoc/>
        public bool IsAbstract { get { return false; } }

        /// <inheritdoc/>
        public bool IsVirtual { get { return false; } }
    }
}
