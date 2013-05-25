using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A code doc model for a property member.
    /// </summary>
    public class CodeDocProperty : CodeDocMemberContentBase, ICodeDocValueMember
    {

        /// <summary>
        /// Creates a new model for a property member.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocProperty(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// Indicates that this property has parameters.
        /// </summary>
        public bool HasParameters { get { return Parameters != null && Parameters.Count > 0; } }

        /// <summary>
        /// Gets the parameters for this property member.
        /// </summary>
        public IList<CodeDocParameter> Parameters { get; set; }

        /// <summary>
        /// Indicates that this property has a getter method.
        /// </summary>
        public bool HasGetter { get { return Getter != null; } }

        /// <summary>
        /// Gets the getter method for this property.
        /// </summary>
        public CodeDocMethod Getter { get; set; }

        /// <summary>
        /// Indicates that this property has a setter method.
        /// </summary>
        public bool HasSetter { get { return Setter != null; } }

        /// <summary>
        /// Gets the setter method for this property.
        /// </summary>
        public CodeDocMethod Setter { get; set; }

        /// <inheritdoc/>
        public ICodeDocMember ValueType { get; set; }

        /// <inheritdoc/>
        public bool HasValueDescriptionContents { get { return ValueDescriptionContents != null && ValueDescriptionContents.Count > 0; } }

        /// <inheritdoc/>
        public IList<XmlDocNode> ValueDescriptionContents { get; set; }

    }
}
