using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code doc generic type parameter model.
    /// </summary>
    public class CodeDocGenericParameter
    {
        /// <summary>
        /// Indicates that this parameter has type constraints.
        /// </summary>
        public bool HasTypeConstraints {
            get { return TypeConstraints != null && TypeConstraints.Count > 0; }
        }

        /// <summary>
        /// Gets the type constraints.
        /// </summary>
        public IList<ICodeDocMember> TypeConstraints { get; set; }

        /// <summary>
        /// The generic type parameter name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates that this model has an XML doc summary.
        /// </summary>
        public bool HasSummary {
            get { return Summary != null; }
        }

        /// <summary>
        /// Gets the XML doc summary for the type parameter.
        /// </summary>
        public XmlDocElement Summary { get; set; }

        /// <summary>
        /// Indicates that this model has XML doc summary contents.
        /// </summary>
        public bool HasSummaryContents {
            get { return HasSummary && Summary.HasChildren; }
        }

        /// <summary>
        /// Gets the XML doc summary contents.
        /// </summary>
        public IList<XmlDocNode> SummaryContents {
            get {
                return HasSummaryContents
                    ? Summary.Children
                    : new XmlDocNode[0];
            }
        }

        /// <summary>
        /// Indicates that this parameter is contravariant.
        /// </summary>
        public bool IsContravariant { get; set; }

        /// <summary>
        /// Indicates that this parameter is covariant.
        /// </summary>
        public bool IsCovariant { get; set; }

        /// <summary>
        /// Indicates that this parameter is constrained to reference types.
        /// </summary>
        public bool HasReferenceTypeConstraint { get; set; }

        /// <summary>
        /// Indicates that this parameter is constrained to value types.
        /// </summary>
        public bool HasNotNullableValueTypeConstraint { get; set; }

        /// <summary>
        /// Indicates that this parameter is constrained to types with default constructors.
        /// </summary>
        public bool HasDefaultConstructorConstraint { get; set; }

        /// <summary>
        /// Indicates that this parameter has any constraints.
        /// </summary>
        public bool HasAnyConstraints {
            get {
                return HasTypeConstraints
                    || HasReferenceTypeConstraint
                    || HasNotNullableValueTypeConstraint
                    || HasDefaultConstructorConstraint;
            }
        }

    }
}
