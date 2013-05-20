using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code doc model for a type.
    /// </summary>
    public class CodeDocType : CodeDocMemberContentBase
    {
        /// <summary>
        /// Creates a new model for a type.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocType(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// Indicates that the type has a base chain of inherited types.
        /// </summary>
        public bool HasBaseChain { get { return BaseChain != null && BaseChain.Count > 0; } }

        /// <summary>
        /// Gets the inherited types.
        /// </summary>
        public IList<CodeDocType> BaseChain { get; set; }

        /// <summary>
        /// Indicates that this type implements interfaces.
        /// </summary>
        public bool HasInterfaces { get { return Interfaces != null && Interfaces.Count > 0; } }

        /// <summary>
        /// Gets the interfaces implemented by this type.
        /// </summary>
        public IList<CodeDocType> Interfaces { get; set; }

        /// <summary>
        /// Indicates that this type has generic parameter.
        /// </summary>
        public bool HasGenericParameters { get { return GenericParameters != null && GenericParameters.Count > 0; } }

        /// <summary>
        /// Gets the generic parameters.
        /// </summary>
        public IList<CodeDocGenericParameter> GenericParameters { get; set; }

        /// <summary>
        /// Indicates that this type is an enumeration type.
        /// </summary>
        public bool IsEnum { get; set; }

        /// <summary>
        /// Indicates that this type is an enumeration type and is attributed as flags.
        /// </summary>
        public bool IsFlagsEnum { get; set; }

        /// <summary>
        /// Indicates that this type is sealed.
        /// </summary>
        public bool IsSealed { get; set; }

        /// <summary>
        /// Indicates that this type is a value type.
        /// </summary>
        public bool IsValueType { get; set; }

        /// <summary>
        /// Indicates that this type contains nested types.
        /// </summary>
        public bool HasNestedTypes { get { return NestedTypes != null && NestedTypes.Count > 0; } }

        /// <summary>
        /// Gets the nested type members for this type.
        /// </summary>
        public IList<ICodeDocMember> NestedTypes { get; set; }

        /// <summary>
        /// Indicates that this type contains nested delegate types.
        /// </summary>
        public bool HasNestedDelegates { get { return NestedDelegates != null && NestedDelegates.Count > 0; } }

        /// <summary>
        /// Gets the nested delegate members for this type.
        /// </summary>
        public IList<ICodeDocMember> NestedDelegates { get; set; }

        /// <summary>
        /// Indicates that this type has constructors.
        /// </summary>
        public bool HasConstructors { get { return Constructors != null && Constructors.Count > 0; } }

        /// <summary>
        /// Gets the constructors for this type.
        /// </summary>
        public IList<ICodeDocMember> Constructors { get; set; }

        /// <summary>
        /// Indicates that this type has methods.
        /// </summary>
        public bool HasMethods { get { return Methods != null && Methods.Count > 0; } }

        /// <summary>
        /// Gets the methods for this type.
        /// </summary>
        public IList<ICodeDocMember> Methods { get; set; }

        /// <summary>
        /// Indicates that this type has operators.
        /// </summary>
        public bool HasOperators { get { return Operators != null && Operators.Count > 0; } }

        /// <summary>
        /// Gets the operators for this type.
        /// </summary>
        public IList<ICodeDocMember> Operators { get; set; }

        /// <summary>
        /// Indicates that this type has property members.
        /// </summary>
        public bool HasProperties { get { return Properties != null && Properties.Count > 0; } }

        /// <summary>
        /// Gets the properties for this type.
        /// </summary>
        public IList<ICodeDocMember> Properties { get; set; }

        /// <summary>
        /// Indicates that this type has field members.
        /// </summary>
        public bool HasFields { get { return Fields != null && Fields.Count > 0; } }

        /// <summary>
        /// Gets the fields for this type.
        /// </summary>
        public IList<ICodeDocMember> Fields { get; set; }

        /// <summary>
        /// Indicates that this type has event members.
        /// </summary>
        public bool HasEvents { get { return Events != null && Events.Count > 0; } }

        /// <summary>
        /// Gets the events for this type.
        /// </summary>
        public IList<ICodeDocMember> Events { get; set; }

    }
}
