using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A code doc model for a method member.
    /// </summary>
    [DataContract]
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
        [IgnoreDataMember]
        public bool HasParameters { get { return Parameters != null && Parameters.Count > 0; } }

        /// <inheritdoc/>
        [DataMember]
        public IList<CodeDocParameter> Parameters { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool HasReturn { get { return Return != null; } }

        /// <inheritdoc/>
        [DataMember]
        public CodeDocParameter Return { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool HasExceptions { get { return Exceptions != null && Exceptions.Count > 0; } }

        /// <inheritdoc/>
        [DataMember]
        public IList<CodeDocException> Exceptions { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool HasEnsures { get { return Ensures != null && Ensures.Count > 0; } }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IList<XmlDocContractElement> Ensures { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool HasNormalTerminationEnsures {
            get {
                return HasEnsures
                    && NormalTerminationEnsures.Any();
            }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IEnumerable<XmlDocContractElement> NormalTerminationEnsures {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<XmlDocContractElement>>() != null);
                return HasEnsures
                    ? Ensures.Where(x => "ENSURES".Equals(x.Name, StringComparison.OrdinalIgnoreCase))
                    : Enumerable.Empty<XmlDocContractElement>();
            }
        }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool HasRequires { get { return Requires != null && Requires.Count > 0; } }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IList<XmlDocContractElement> Requires { get; set; }

        /// <summary>
        /// Indicates that this method has generic parameter.
        /// </summary>
        [IgnoreDataMember]
        public bool HasGenericParameters { get { return GenericParameters != null && GenericParameters.Count > 0; } }

        /// <summary>
        /// Gets the generic parameters.
        /// </summary>
        [DataMember]
        public IList<CodeDocGenericParameter> GenericParameters { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsPure { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsExtensionMethod { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsOperatorOverload { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsSealed { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsAbstract { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsVirtual { get; set; }

    }
}
