using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CodeDoc.Utility;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A code doc member data provider that aggregates attributes from multiple providers.
    /// </summary>
    public class CodeDocMemberDataProviderCollection :
        Collection<ICodeDocMemberDataProvider>,
        ICodeDocMemberDataProvider
    {

        /// <summary>
        /// A default empty provider collection.
        /// </summary>
        public CodeDocMemberDataProviderCollection()
            : this(new List<ICodeDocMemberDataProvider>()) { }

        /// <summary>
        /// Creates a provider collection containing the given <paramref name="providers"/>.
        /// </summary>
        /// <param name="providers">The providers that will initially be within the collection.</param>
        public CodeDocMemberDataProviderCollection(IEnumerable<ICodeDocMemberDataProvider> providers)
            : base(providers.ToList()) {
            Contract.Requires(providers != null);
            MergeGroups = false;
        }

        /// <summary>
        /// Indicates that some member data attributes should be merged when there are multiple sources.
        /// </summary>
        public virtual bool MergeGroups { get; private set; }

        /// <inheritdoc/>
        public virtual bool HasExamples { get { return this.Any(x => x.HasExamples); } }

        /// <summary>
        /// Gets the first set of examples or the merged examples based on <see cref="MergeGroups"/>.
        /// </summary>
        /// <returns>A collection of examples.</returns>
        public virtual IEnumerable<XmlDocElement> GetExamples() {
            var relevantProviders = this.Where(x => x.HasExamples);
            if (MergeGroups)
                return relevantProviders.SelectMany(x => x.GetExamples());
            return relevantProviders
                .Select(x => x.GetExamples())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocElement>();
        }

        /// <inheritdoc/>
        public virtual bool HasPermissions { get { return this.Any(x => x.HasPermissions); } }

        /// <summary>
        /// Gets the first set of permissions or the merged permissions based on <see cref="MergeGroups"/>.
        /// </summary>
        /// <returns>A collection of permissions.</returns>
        public virtual IEnumerable<XmlDocRefElement> GetPermissions() {
            var relevantProviders = this.Where(x => x.HasPermissions);
            if (MergeGroups)
                return relevantProviders.SelectMany(x => x.GetPermissions());
            return relevantProviders
                .Select(x => x.GetPermissions())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocRefElement>();
        }

        /// <inheritdoc/>
        public virtual bool HasSummaryContents {
            get { return this.Any(x => x.HasSummaryContents); }
        }

        /// <summary>
        /// Gets the first set of summary contents.
        /// </summary>
        /// <returns>Summary contents.</returns>
        public virtual IEnumerable<XmlDocNode> GetSummaryContents() {
            return this.Where(x => x.HasSummaryContents)
                .Select(x => x.GetSummaryContents())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }

        /// <inheritdoc/>
        public virtual bool HasValueDescriptionContents {
            get { return this.Any(x => x.HasValueDescriptionContents); }
        }

        /// <summary>
        /// Gets the first set of value description contents.
        /// </summary>
        /// <returns>Value description contents.</returns>
        public virtual IEnumerable<XmlDocNode> GeValueDescriptionContents() {
            return this.Where(x => x.HasValueDescriptionContents)
                .Select(x => x.GeValueDescriptionContents())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }

        /// <inheritdoc/>
        public virtual bool HasRemarks {
            get { return this.Any(x => x.HasRemarks); }
        }

        /// <summary>
        /// Gets the first set of remarks or the merged remarks based on <see cref="MergeGroups"/>.
        /// </summary>
        /// <returns>A collection of remarks.</returns>
        public virtual IEnumerable<XmlDocElement> GetRemarks() {
            var relevantProviders = this.Where(x => x.HasRemarks);
            if (MergeGroups)
                return relevantProviders.SelectMany(x => x.GetRemarks());
            return relevantProviders
                .Select(x => x.GetRemarks())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocElement>();
        }

        /// <inheritdoc/>
        public virtual bool HasSeeAlso {
            get { return this.Any(x => x.HasSeeAlso); }
        }

        /// <summary>
        /// Gets the first set of see also elements or the merged see also elements based on <see cref="MergeGroups"/>.
        /// </summary>
        /// <returns>A collection of see also elements.</returns>
        public virtual IEnumerable<XmlDocRefElement> GetSeeAlsos() {
            var relevantProviders = this.Where(x => x.HasSeeAlso);
            if (MergeGroups)
                return relevantProviders.SelectMany(x => x.GetSeeAlsos());
            return relevantProviders
                .Select(x => x.GetSeeAlsos())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocRefElement>();
        }

        /// <inheritdoc/>
        public virtual bool? IsPure {
            get { return this.Select(x => x.IsPure).FirstSetNullableOrDefault(); }
        }

        /// <inheritdoc/>
        public virtual ExternalVisibility.ExternalVisibilityKind? ExternalVisibility {
            get { return this.Select(x => x.ExternalVisibility).FirstSetNullableOrDefault(); }
        }

        /// <inheritdoc/>
        public virtual bool? IsStatic {
            get { return this.Select(x => x.IsStatic).FirstSetNullableOrDefault(); }
        }

        /// <inheritdoc/>
        public virtual bool? IsObsolete {
            get { return this.Select(x => x.IsObsolete).FirstSetNullableOrDefault(); }
        }

        /// <inheritdoc/>
        public virtual bool HasParameterSummaryContents(string parameterName) {
            return this.Any(x => x.HasParameterSummaryContents(parameterName));
        }

        /// <summary>
        /// Gets the first parameter summary contents for the given parameter name.
        /// </summary>
        /// <param name="parameterName">The target parameter name.</param>
        /// <returns>Parameter summary contents.</returns>
        public virtual IEnumerable<XmlDocNode> GetParameterSummaryContents(string parameterName) {
            return this
                .Where(x => x.HasParameterSummaryContents(parameterName))
                .Select(x => x.GetParameterSummaryContents(parameterName))
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }

        /// <inheritdoc/>
        public virtual bool HasReturnSummaryContents {
            get { return this.Any(x => x.HasReturnSummaryContents); }
        }

        /// <summary>
        /// Gets the first return summary contents for the return parameter.
        /// </summary>
        /// <returns>Parameter summary contents.</returns>
        public virtual IEnumerable<XmlDocNode> GetReturnSummaryContents() {
            return this
                .Where(x => x.HasReturnSummaryContents)
                .Select(x => x.GetReturnSummaryContents())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }

        /// <summary>
        /// Finds the first set <see cref="ICodeDocMemberDataProvider.RequiresParameterNotEverNull(System.String)"/> value.
        /// </summary>
        /// <param name="parameterName">The target parameter name.</param>
        /// <returns>A value indicating if the parameter is required to not ever be null.</returns>
        public virtual bool? RequiresParameterNotEverNull(string parameterName) {
            return this.Select(x => x.RequiresParameterNotEverNull(parameterName)).FirstSetNullableOrDefault();
        }

        /// <summary>
        /// Finds the first set <see cref="ICodeDocMemberDataProvider.EnsuresResultNotEverNull"/> value.
        /// </summary>
        public virtual bool? EnsuresResultNotEverNull {
            get {
                return this
                    .Select(x => x.EnsuresResultNotEverNull)
                    .FirstSetNullableOrDefault();
            }
        }

        /// <inheritdoc/>
        public virtual bool HasGenericTypeSummaryContents(string typeParameterName) {
            return this.Any(x => x.HasGenericTypeSummaryContents(typeParameterName));
        }

        /// <summary>
        /// Gets the first type parameter summary contents for the given parameter name.
        /// </summary>
        /// <param name="typeParameterName">The target type parameter name.</param>
        /// <returns>Type parameter summary contents.</returns>
        public virtual IEnumerable<XmlDocNode> GetGenericTypeSummaryContents(string typeParameterName) {
            return this
                .Where(x => x.HasGenericTypeSummaryContents(typeParameterName))
                .Select(x => x.GetGenericTypeSummaryContents(typeParameterName))
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }

        /// <inheritdoc/>
        public virtual bool HasExceptions {
            get { return this.Any(x => x.HasExceptions); }
        }

        /// <summary>
        /// Gets the first set of exceptions.
        /// </summary>
        /// <returns>A set of exceptions.</returns>
        public virtual IEnumerable<XmlDocRefElement> GetExceptions() {
            return this
                .Where(x => x.HasExceptions)
                .Select(x => x.GetExceptions())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocRefElement>();
        }

        /// <inheritdoc/>
        public virtual bool HasEnsures {
            get { return this.Any(x => x.HasEnsures); }
        }

        /// <summary>
        /// Gets the first set of code contract ensures.
        /// </summary>
        /// <returns>A set of code contract ensures.</returns>
        public virtual IEnumerable<XmlDocContractElement> GetEnsures() {
            return this
                .Where(x => x.HasEnsures)
                .Select(x => x.GetEnsures())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocContractElement>();
        }

        /// <inheritdoc/>
        public virtual bool HasRequires {
            get { return this.Any(x => x.HasRequires); }
        }

        /// <summary>
        /// Gets the first set of code contract requires.
        /// </summary>
        /// <returns>A set of code contract requires.</returns>
        public virtual IEnumerable<XmlDocContractElement> GetRequires() {
            return this
                .Where(x => x.HasRequires)
                .Select(x => x.GetRequires())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocContractElement>();
        }

        /// <inheritdoc/>
        public virtual bool HasInvariants {
            get { return this.Any(x => x.HasInvariants); }
        }

        /// <summary>
        /// Gets the first set of code contract invariants.
        /// </summary>
        /// <returns>A set of code contract invariants.</returns>
        public virtual IEnumerable<XmlDocContractElement> GetInvariants() {
            return this
                .Where(x => x.HasInvariants)
                .Select(x => x.GetInvariants())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocContractElement>();
        }
    }
}
