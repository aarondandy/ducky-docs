using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// Provides member attribute data from XML documentation files.
    /// </summary>
    public class CodeDocMemberXmlDataProvider : ICodeDocMemberDataProvider
    {

        /// <summary>
        /// Creates a provider based on the given XML documentation for a member.
        /// </summary>
        /// <param name="xmlDoc">The member XML documentation to wrap.</param>
        public CodeDocMemberXmlDataProvider(XmlDocMember xmlDoc) {
            if(xmlDoc == null) throw new ArgumentNullException("xmlDoc");
            XmlDoc = xmlDoc;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(XmlDoc != null);
        }

        /// <summary>
        /// The wrapped member XML documentation.
        /// </summary>
        public XmlDocMember XmlDoc { get; private set; }

        /// <inheritdoc/>
        public bool HasExamples { get { return XmlDoc.HasExampleElements; } }

        /// <inheritdoc/>
        public IEnumerable<XmlDocElement> GetExamples() {
            return XmlDoc.ExampleElements;
        }

        /// <inheritdoc/>
        public bool HasSummaryContents { get { return XmlDoc.HasSummaryContents; } }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GetSummaryContents() {
            return XmlDoc.SummaryContents;
        }

        /// <inheritdoc/>
        public bool HasValueDescriptionContents { get { return XmlDoc.HasValueContents; } }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GeValueDescriptionContents() {
            return XmlDoc.ValueContents;
        }

        /// <inheritdoc/>
        public bool HasPermissions {
            get { return XmlDoc.HasPermissionElements; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocRefElement> GetPermissions() {
            return XmlDoc.PermissionElements;
        }

        /// <inheritdoc/>
        public bool HasRemarks {
            get { return XmlDoc.HasRemarksElements; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocElement> GetRemarks() {
            return XmlDoc.RemarksElements;
        }

        /// <inheritdoc/>
        public bool HasSeeAlso {
            get { return XmlDoc.HasSeeAlsoElements; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocRefElement> GetSeeAlsos() {
            return XmlDoc.SeeAlsoElements;
        }

        /// <inheritdoc/>
        public bool? IsPure {
            get {
                return XmlDoc.HasPureElement ? true : default(bool?);
            }
        }

        /// <inheritdoc/>
        public ExternalVisibility.ExternalVisibilityKind? ExternalVisibility {
            get { return null; }
        }

        /// <inheritdoc/>
        public bool? IsStatic {
            get { return null; }
        }

        /// <inheritdoc/>
        public bool? IsObsolete {
            get{ return null; }
        }

        /// <inheritdoc/>
        public bool HasParameterSummaryContents(string parameterName) {
            if (!XmlDoc.HasParameterSummaries)
                return false;
            var parameterSummary = XmlDoc.GetParameterSummary(parameterName);
            return parameterSummary != null && parameterSummary.HasChildren;
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GetParameterSummaryContents(string parameterName) {
            if (!XmlDoc.HasParameterSummaries)
                return Enumerable.Empty<XmlDocNode>();
            var parameterSummary = XmlDoc.GetParameterSummary(parameterName);
            return parameterSummary == null ? Enumerable.Empty<XmlDocNode>() : parameterSummary.Children;
        }

        /// <inheritdoc/>
        public bool HasReturnSummaryContents {
            get { return XmlDoc.HasReturnsContents; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GetReturnSummaryContents() {
            return XmlDoc.ReturnsContents;
        }

        /// <inheritdoc/>
        public bool? RequiresParameterNotEverNull(string parameterName) {
            if (XmlDoc.HasRequiresElements && XmlDoc.RequiresElements.Any(r => r.RequiresParameterNotEverNull(parameterName)))
                return true;

            return null;
        }

        /// <inheritdoc/>
        public bool? EnsuresResultNotEverNull {
            get {
                if (XmlDoc.HasEnsuresElements && XmlDoc.EnsuresElements.Any(r => r.EnsuresResultNotEverNull))
                    return true;
                return null;
            }
        }

        /// <inheritdoc/>
        public bool HasGenericTypeSummaryContents(string typeParameterName) {
            if (!XmlDoc.HasTypeParameterSummaries)
                return false;
            var summary = XmlDoc.GetTypeParameterSummary(typeParameterName);
            return summary != null && summary.HasChildren;
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GetGenericTypeSummaryContents(string typeParameterName) {
            if (!XmlDoc.HasTypeParameterSummaries)
                return Enumerable.Empty<XmlDocNode>();
            var summary = XmlDoc.GetTypeParameterSummary(typeParameterName);
            return summary != null
                ? summary.Children
                : Enumerable.Empty<XmlDocNode>();
        }

        /// <inheritdoc/>
        public bool HasExceptions {
            get { return XmlDoc.HasExceptionElements; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocRefElement> GetExceptions() {
            return XmlDoc.ExceptionElements;
        }

        /// <inheritdoc/>
        public bool HasEnsures {
            get { return XmlDoc.HasEnsuresElements; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocContractElement> GetEnsures() {
            return XmlDoc.EnsuresElements;
        }

        /// <inheritdoc/>
        public bool HasRequires {
            get { return XmlDoc.HasRequiresElements; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocContractElement> GetRequires() {
            return XmlDoc.RequiresElements;
        }

        /// <inheritdoc/>
        public bool HasInvariants {
            get { return XmlDoc.HasInvariantElements; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocContractElement> GetInvariants() {
            return XmlDoc.InvariantElements;
        }
    }
}
