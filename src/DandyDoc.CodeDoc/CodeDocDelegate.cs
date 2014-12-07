using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using DuckyDocs.CRef;
using DuckyDocs.XmlDoc;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// A code doc model for a delegate member.
    /// </summary>
    [DataContract]
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
        public bool HasNormalTerminationEnsures { get { return HasEnsures && NormalTerminationEnsures.Any(); } }

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

        /// <inheritdoc/>
        [DataMember]
        public bool? IsPure { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsExtensionMethod { get { return false; } }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsOperatorOverload { get { return false; } }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsAbstract { get { return false; } }

        /// <inheritdoc/>
        [DataMember]
        public bool? IsVirtual { get { return false; } }
    }
}
