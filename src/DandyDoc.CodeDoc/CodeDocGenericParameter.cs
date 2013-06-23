using System.Collections.Generic;
using System.Runtime.Serialization;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code doc generic type parameter model.
    /// </summary>
    [DataContract]
    public class CodeDocGenericParameter
    {

        /// <summary>
        /// Creates a default generic parameter model.
        /// </summary>
        public CodeDocGenericParameter(){}

        /// <summary>
        /// Creates a gemeric parameter model with the given name.
        /// </summary>
        /// <param name="name"></param>
        public CodeDocGenericParameter(string name) {
            Name = name;
        }

        /// <summary>
        /// Indicates that this parameter has type constraints.
        /// </summary>
        [IgnoreDataMember]
        public bool HasTypeConstraints {
            get { return TypeConstraints != null && TypeConstraints.Count > 0; }
        }

        /// <summary>
        /// Gets the type constraints.
        /// </summary>
        [DataMember]
        public IList<CodeDocType> TypeConstraints { get; set; }

        /// <summary>
        /// The generic type parameter name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Indicates that this model has XML doc summary contents.
        /// </summary>
        [IgnoreDataMember]
        public bool HasSummaryContents {
            get { return SummaryContents != null && SummaryContents.Count > 0; }
        }

        /// <summary>
        /// Gets the XML doc summary contents.
        /// </summary>
        [IgnoreDataMember]
        public IList<XmlDocNode> SummaryContents { get; set; }

        /// <summary>
        /// Indicates that this parameter is contravariant.
        /// </summary>
        [DataMember]
        public bool? IsContravariant { get; set; }

        /// <summary>
        /// Indicates that this parameter is covariant.
        /// </summary>
        [DataMember]
        public bool? IsCovariant { get; set; }

        /// <summary>
        /// Indicates that this parameter is constrained to reference types.
        /// </summary>
        [DataMember]
        public bool? HasReferenceTypeConstraint { get; set; }

        /// <summary>
        /// Indicates that this parameter is constrained to value types.
        /// </summary>
        [DataMember]
        public bool? HasNotNullableValueTypeConstraint { get; set; }

        /// <summary>
        /// Indicates that this parameter is constrained to types with default constructors.
        /// </summary>
        [DataMember]
        public bool? HasDefaultConstructorConstraint { get; set; }

        /// <summary>
        /// Indicates that this parameter has any constraints.
        /// </summary>
        [IgnoreDataMember]
        public bool HasAnyConstraints {
            get {
                return HasTypeConstraints
                    || HasReferenceTypeConstraint.GetValueOrDefault()
                    || HasNotNullableValueTypeConstraint.GetValueOrDefault()
                    || HasDefaultConstructorConstraint.GetValueOrDefault();
            }
        }

    }
}
