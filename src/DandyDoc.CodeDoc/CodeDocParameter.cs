using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code doc parameter model.
    /// </summary>
    public class CodeDocParameter
    {

        /// <summary>
        /// Creates a new parameter code doc model.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="parameterType">The parameter type.</param>
        /// <param name="summary">The optional XML doc summary element.</param>
        public CodeDocParameter(string name, ICodeDocMember parameterType, XmlDocElement summary = null) {
            Name = name;
            ParameterType = parameterType;
            Summary = summary;
        }

        /// <summary>
        /// The parameter name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The parameter type.
        /// </summary>
        public ICodeDocMember ParameterType { get; private set; }

        /// <summary>
        /// Indicates that the parameter has an XML doc summary element.
        /// </summary>
        public bool HasSummary {
            get { return Summary != null; }
        }

        /// <summary>
        /// Gets the XML doc summary element.
        /// </summary>
        public XmlDocElement Summary { get; private set; }

        /// <summary>
        /// Indicates that the parameter has XML doc summary contents.
        /// </summary>
        public bool HasSummaryContents {
            get { return Summary != null && Summary.HasChildren; }
        }

        /// <summary>
        /// Gets the XML doc summary contents.
        /// </summary>
        public IList<XmlDocNode> SummaryContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                return HasSummaryContents
                    ? Summary.Children
                    : new XmlDocNode[0];
            }
        }

        /// <summary>
        /// Indicates that this is an out parameter.
        /// </summary>
        public bool IsOut { get; set; }

        /// <summary>
        /// Indicates that this parameter is passed by reference.
        /// </summary>
        public bool IsByRef { get; set; }

        /// <summary>
        /// Indicates that this parameter is null restricted.
        /// </summary>
        public bool? NullRestricted { get; set; }

        /// <summary>
        /// Indicates that this parameter type is a reference type.
        /// </summary>
        /// <remarks>
        /// Be careful not to confuse this with <see cref="IsByRef"/>.
        /// </remarks>
        public bool? IsReferenceType { get; set; }

    }
}
