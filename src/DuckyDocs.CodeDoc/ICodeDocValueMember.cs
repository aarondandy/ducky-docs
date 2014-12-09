using System;
using System.Collections.Generic;
using DuckyDocs.XmlDoc;

namespace DuckyDocs.CodeDoc
{
    /// <summary>
    /// Defines a value based member such as a field or property.
    /// </summary>
    public interface ICodeDocValueMember
    {

        /// <summary>
        /// The type of the value.
        /// </summary>
        ICodeDocMember ValueType { get; }

        /// <summary>
        /// Indicates that this member has value description contents.
        /// </summary>
        bool HasValueDescriptionContents { get; }

        /// <summary>
        /// Gets the member value description contents.
        /// </summary>
        IList<XmlDocNode> ValueDescriptionContents { get; }

    }
}
