using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A code doc field model.
    /// </summary>
    [DataContract]
    public class CodeDocField : CodeDocMemberContentBase, ICodeDocValueMember
    {

        /// <summary>
        /// Creates a new model for a field member.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocField(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        /// <inheritdoc/>
        [DataMember]
        public ICodeDocMember ValueType { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool HasValueDescriptionContents { get { return ValueDescriptionContents != null && ValueDescriptionContents.Count > 0; } }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IList<XmlDocNode> ValueDescriptionContents { get; set; }

        /// <summary>
        /// Indicates that this field is a literal value.
        /// </summary>
        [DataMember]
        public bool? IsLiteral { get; set; }

        /// <summary>
        /// Indicates that this field is an init-only field.
        /// </summary>
        [DataMember]
        public bool? IsInitOnly { get; set; }

    }
}
