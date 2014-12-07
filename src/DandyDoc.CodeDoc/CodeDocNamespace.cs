using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using DuckyDocs.CRef;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// A code doc namespace model.
    /// </summary>
    [DataContract]
    public class CodeDocNamespace : CodeDocSimpleNamespace
    {

        /// <summary>
        /// Creates a new code doc namespace model.
        /// </summary>
        /// <param name="cRef">The namespace code reference.</param>
        public CodeDocNamespace(CRefIdentifier cRef) : base(cRef) {
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// All exposed types within this namespace.
        /// </summary>
        [DataMember]
        public IList<CodeDocType> Types { get; set; }

        /// <summary>
        /// All assemblies for the exposed types within this namespace.
        /// </summary>
        [DataMember]
        public IList<CodeDocSimpleAssembly> Assemblies { get; set; }

    }
}
