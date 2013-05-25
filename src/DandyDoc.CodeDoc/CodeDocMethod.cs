using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A code doc model for a method member.
    /// </summary>
    public class CodeDocMethod : CodeDocMemberContentBase, ICodeDocInvokable
    {

        /// <summary>
        /// Creates a new model for a method.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocMethod(CRefIdentifier cRef) : base(cRef){
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

        /// <summary>
        /// Indicates that this method has generic parameter.
        /// </summary>
        public bool HasGenericParameters { get { return GenericParameters != null && GenericParameters.Count > 0; } }

        /// <summary>
        /// Gets the generic parameters.
        /// </summary>
        public IList<CodeDocGenericParameter> GenericParameters { get; set; }

        /// <inheritdoc/>
        public bool? IsPure { get; set; }

        /// <inheritdoc/>
        public bool? IsExtensionMethod { get; set; }

        /// <inheritdoc/>
        public bool? IsOperatorOverload { get; set; }

        /// <inheritdoc/>
        public bool? IsSealed { get; set; }

        /// <inheritdoc/>
        public bool? IsAbstract { get; set; }

        /// <inheritdoc/>
        public bool? IsVirtual { get; set; }

    }
}
