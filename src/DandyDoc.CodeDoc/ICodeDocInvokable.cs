using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// Defines an invokable member such as a method or delegate.
    /// </summary>
    public interface ICodeDocInvokable
    {

        /// <summary>
        /// Indicates that this member has any parameters.
        /// </summary>
        bool HasParameters { get; }

        /// <summary>
        /// Gets the parameters for the member.
        /// </summary>
        IList<CodeDocParameter> Parameters { get; }

        /// <summary>
        /// Indicates that this member has a return.
        /// </summary>
        bool HasReturn { get; }

        /// <summary>
        /// Gets the return parameter for the member.
        /// </summary>
        CodeDocParameter Return { get; }

        /// <summary>
        /// Indicates that this member declares any exceptions that may be thrown.
        /// </summary>
        bool HasExceptions { get; }

        /// <summary>
        /// Gets the exceptions that may be thrown.
        /// </summary>
        IList<CodeDocException> Exceptions { get; }

        /// <summary>
        /// Indicates that this member has any ensures contracts.
        /// </summary>
        bool HasEnsures { get; }

        /// <summary>
        /// Gets all ensures contracts for this member.
        /// </summary>
        IList<XmlDocContractElement> Ensures { get; }

        /// <summary>
        /// Indicates that this member has any ensures contracts for normal termination.
        /// </summary>
        bool HasNormalTerminationEnsures { get; }

        /// <summary>
        /// Gets the ensures contracts for this member related to normal termination.
        /// </summary>
        IEnumerable<XmlDocContractElement> NormalTerminationEnsures { get; }

        /// <summary>
        /// Indicates that this member has requires contracts.
        /// </summary>
        bool HasRequires { get; }

        /// <summary>
        /// Gets all requires contracts for this member.
        /// </summary>
        IList<XmlDocContractElement> Requires { get; }

        /// <summary>
        /// Indicates that this member is pure.
        /// </summary>
        bool? IsPure { get; }

        /// <summary>
        /// Indicates that this member is an extension method.
        /// </summary>
        bool? IsExtensionMethod { get; }

        /// <summary>
        /// Indicates that this member is an operator overload.
        /// </summary>
        bool? IsOperatorOverload { get; }

        /// <summary>
        /// Indicates that this member is sealed.
        /// </summary>
        bool? IsSealed { get; }

        /// <summary>
        /// Indicates that this member is abstract.
        /// </summary>
        bool? IsAbstract { get; }

        /// <summary>
        /// Indicates that this member is virtual.
        /// </summary>
        bool? IsVirtual { get; }

    }
}
