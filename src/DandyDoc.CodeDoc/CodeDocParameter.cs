using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using DuckyDocs.XmlDoc;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// A code doc parameter model.
    /// </summary>
    [DataContract]
    public class CodeDocParameter
    {

        /// <summary>
        /// Creates a new parameter code doc model.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="parameterType">The parameter type.</param>
        public CodeDocParameter(string name, ICodeDocMember parameterType) {
            if(String.IsNullOrEmpty(name)) throw new ArgumentException("Parameter name must contain characters.","name");
            Name = name;
            ParameterType = parameterType;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
        }

        /// <summary>
        /// The parameter name.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// The parameter type.
        /// </summary>
        [DataMember]
        public ICodeDocMember ParameterType { get; private set; }

        /// <summary>
        /// Indicates that the parameter has XML doc summary contents.
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
        /// Indicates that this is an out parameter.
        /// </summary>
        [DataMember]
        public bool? IsOut { get; set; }

        /// <summary>
        /// Indicates that this parameter is passed by reference.
        /// </summary>
        [DataMember]
        public bool? IsByRef { get; set; }

        /// <summary>
        /// Indicates that this parameter is null restricted.
        /// </summary>
        [DataMember]
        public bool? NullRestricted { get; set; }

        /// <summary>
        /// Indicates that this parameter type is a reference type.
        /// </summary>
        /// <remarks>
        /// Be careful not to confuse this with <see cref="IsByRef"/>.
        /// </remarks>
        [DataMember]
        public bool? IsReferenceType { get; set; }

    }
}
