using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using DuckyDocs.CRef;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// Code doc member representing an assembly.
    /// </summary>
    [DataContract]
    public class CodeDocSimpleAssembly : CodeDocSimpleMember
    {
        /// <summary>
        /// Creates a new assembly with the given cRef identifier.
        /// </summary>
        /// <param name="cRef">The assembly code reference.</param>
        /// <remarks>
        /// Assembly code references should be prefixed with 'A:' followed by the assembly short name or full name.
        /// </remarks>
        public CodeDocSimpleAssembly(CRefIdentifier cRef) : base(cRef) { 
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// The file name of the assembly.
        /// </summary>
        /// <remarks>
        /// The assembly file name should not be a full path but just the file name such as <c>MyLibrary.dll</c> .
        /// </remarks>
        [DataMember]
        public string AssemblyFileName { get; set; }

        /// <summary>
        /// The code references for the namespaces related to all exposed types within this assembly.
        /// </summary>
        [IgnoreDataMember]
        public IList<CRefIdentifier> NamespaceCRefs { get; set; }

        /// <summary>
        /// The code references for all exposed types within this assembly. 
        /// </summary>
        [IgnoreDataMember]
        public IList<CRefIdentifier> TypeCRefs { get; set; }
    }

}
