using System.Collections.Generic;
using DuckyDocs.ExternalVisibility;
using DuckyDocs.XmlDoc;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// Provides data for a code doc member.
    /// </summary>
    public interface ICodeDocMemberDataProvider
    {
        /// <summary>
        /// Determines if there are summary contents.
        /// </summary>
        bool HasSummaryContents { get; }

        /// <summary>
        /// Gets the summary contents.
        /// </summary>
        /// <returns>The summary content nodes.</returns>
        IEnumerable<XmlDocNode> GetSummaryContents();

        /// <summary>
        /// Determines if there are examples.
        /// </summary>
        bool HasExamples { get; }

        /// <summary>
        /// Gets the example elements.
        /// </summary>
        /// <returns>The example elements.</returns>
        IEnumerable<XmlDocElement> GetExamples();

        /// <summary>
        /// Determines if there are permissions.
        /// </summary>
        bool HasPermissions { get; }

        /// <summary>
        /// Gets the permissions elements.
        /// </summary>
        /// <returns>The permissions elements.</returns>
        IEnumerable<XmlDocRefElement> GetPermissions();

        /// <summary>
        /// Determines if the member has value description contents that can be used to describe fields or properties.
        /// </summary>
        bool HasValueDescriptionContents { get; }

        /// <summary>
        /// Gets the value description contents that can be used to describe fields or properties.
        /// </summary>
        /// <returns>The value description contents.</returns>
        IEnumerable<XmlDocNode> GeValueDescriptionContents();

        /// <summary>
        /// Determines if this provider has any remarks sections.
        /// </summary>
        bool HasRemarks { get; }

        /// <summary>
        /// Gets the remarks sections from this provider that can be used to add detailed documentation to a method.
        /// </summary>
        /// <returns>A collection of remarks sections.</returns>
        IEnumerable<XmlDocElement> GetRemarks();

        /// <summary>
        /// Determines if this provider has any see also elements.
        /// </summary>
        bool HasSeeAlso { get; }

        /// <summary>
        /// Gets the see also elements that this provider offers.
        /// </summary>
        /// <returns>A collection of see also element.</returns>
        IEnumerable<XmlDocRefElement> GetSeeAlsos();

        /// <summary>
        /// Indicates any knowledge of purity for this member.
        /// </summary>
        /// <value>A null value indicates that this provider does not offer a value.</value>
        /// <remarks>
        /// The value of this property should be treated as unknown when null.
        /// </remarks>
        bool? IsPure { get; }

        /// <summary>
        /// Determines the external visibility of a member.
        /// </summary>
        /// <remarks>
        /// The value of this property should be treated as unknown when null.
        /// </remarks>
        ExternalVisibilityKind? ExternalVisibility { get; }

        /// <summary>
        /// Indicates that the related member is static.
        /// </summary>
        /// <remarks>
        /// The value of this property should be treated as unknown when null.
        /// </remarks>
        bool? IsStatic { get; }

        /// <summary>
        /// Indicates that the related member is obsolete or deprecated.
        /// </summary>
        /// <remarks>
        /// The value of this property should be treated as unknown when null.
        /// </remarks>
        bool? IsObsolete { get; }

        /// <summary>
        /// Determines if this provider offers summary contents for the targeted parameter of the related member.
        /// </summary>
        /// <param name="parameterName">The name of the target parameter.</param>
        /// <returns><c>true</c> if summary contents are found.</returns>
        bool HasParameterSummaryContents(string parameterName);

        /// <summary>
        /// Gets the summary contents offered for the targeted parameter of the related member.
        /// </summary>
        /// <param name="parameterName">The name of the target parameter.</param>
        /// <returns>Summary contents.</returns>
        IEnumerable<XmlDocNode> GetParameterSummaryContents(string parameterName);

        /// <summary>
        /// Determines if this provider offers return summary contents for the related member.
        /// </summary>
        bool HasReturnSummaryContents { get; }

        /// <summary>
        /// Gets the return summary contents for the related member.
        /// </summary>
        /// <returns>Summary contents.</returns>
        IEnumerable<XmlDocNode> GetReturnSummaryContents();

        /// <summary>
        /// Determines if this provider offers summary contents for the targeted type parameter of the related member.
        /// </summary>
        /// <param name="typeParameterName">The name of the targeted type parameter.</param>
        /// <returns><c>true</c> when summary contents are found.</returns>
        bool HasGenericTypeSummaryContents(string typeParameterName);

        /// <summary>
        /// Gets the summary contents for the targeted type parameter of the related member.
        /// </summary>
        /// <param name="typeParameterName">The name of the targeted type parameter.</param>
        /// <returns>Summary contents.</returns>
        IEnumerable<XmlDocNode> GetGenericTypeSummaryContents(string typeParameterName);

        /// <summary>
        /// Indicates that the target parameter is null restricted.
        /// </summary>
        /// <remarks>
        /// The value of this property should be treated as unknown when null.
        /// </remarks>
        bool? RequiresParameterNotEverNull(string parameterName);

        /// <summary>
        /// Indicates that the result of the related member is null restricted.
        /// </summary>
        /// <remarks>
        /// The value of this property should be treated as unknown when null.
        /// </remarks>
        bool? EnsuresResultNotEverNull { get; }

        /// <summary>
        /// Determines if the related member has documented exceptions.
        /// </summary>
        bool HasExceptions { get; }

        /// <summary>
        /// Gets the documented exceptions for the related member.
        /// </summary>
        /// <returns></returns>
        IEnumerable<XmlDocRefElement> GetExceptions();

        /// <summary>
        /// Determines if the related member has any code contract ensures.
        /// </summary>
        bool HasEnsures { get; }

        /// <summary>
        /// Gets any code contract ensures for the related member.
        /// </summary>
        /// <returns>Code contract elements.</returns>
        IEnumerable<XmlDocContractElement> GetEnsures();

        /// <summary>
        /// Determines if the related member has any code contract requires.
        /// </summary>
        bool HasRequires { get; }

        /// <summary>
        /// Gets any code contract requires for the related member.
        /// </summary>
        /// <returns>Code contract elements.</returns>
        IEnumerable<XmlDocContractElement> GetRequires();

        /// <summary>
        /// Determines if the related member has any code contract invariants.
        /// </summary>
        bool HasInvariants { get; }

        /// <summary>
        /// Gets any code contract invariants for the related member.
        /// </summary>
        /// <returns>Code contract elements.</returns>
        IEnumerable<XmlDocContractElement> GetInvariants();

    }
}
