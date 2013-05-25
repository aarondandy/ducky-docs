using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.ExternalVisibility;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code doc member data provider that wraps an existing code doc model.
    /// </summary>
    public class CodeDocMemberDataProvider : ICodeDocMemberDataProvider
    {

        /// <summary>
        /// Creates a new member data provider for the given model.
        /// </summary>
        /// <param name="core">The model to be wrapped.</param>
        public CodeDocMemberDataProvider(ICodeDocMember core) {
            if(core == null) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            Core = core;
            ContentBase = core as CodeDocMemberContentBase;
            ValueMember = core as ICodeDocValueMember;
            Invokable = core as ICodeDocInvokable;
            Type = core as CodeDocType;
            Method = core as CodeDocMethod;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Core != null);
        }

        /// <summary>
        /// The core wrapped member model.
        /// </summary>
        public ICodeDocMember Core { get; private set; }

        /// <summary>
        /// Determines if the core model is a content base model.
        /// </summary>
        public bool IsContentBase { get { return ContentBase != null; } }

        /// <summary>
        /// Gets the content model.
        /// </summary>
        public CodeDocMemberContentBase ContentBase { get; private set; }

        /// <summary>
        /// Determines if the core model is a value member model.
        /// </summary>
        public bool IsValueMember { get { return ValueMember != null; } }

        /// <summary>
        /// Gets the value member model.
        /// </summary>
        public ICodeDocValueMember ValueMember { get; private set; }

        /// <summary>
        /// Determines if the core model is an invokable model.
        /// </summary>
        public bool IsInvokable { get { return Invokable != null; } }

        /// <summary>
        /// Gets the invokable member model.
        /// </summary>
        public ICodeDocInvokable Invokable { get; private set; }

        /// <summary>
        /// Determines if the core model is a type model.
        /// </summary>
        public bool IsType { get { return Type != null; } }

        /// <summary>
        /// Gets the type member model.
        /// </summary>
        public CodeDocType Type { get; private set; }

        /// <summary>
        /// Determines if the core model is a method model.
        /// </summary>
        public bool IsMethod { get { return Method != null; } }

        /// <summary>
        /// Gets the method member model.
        /// </summary>
        public CodeDocMethod Method { get; private set; }

        /// <inheritdoc/>
        public bool HasSummaryContents {
            get { return Core.HasSummaryContents; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GetSummaryContents() {
            return Core.SummaryContents;
        }

        /// <inheritdoc/>
        public bool HasExamples {
            get { return IsContentBase && ContentBase.HasExamples; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocElement> GetExamples() {
            return HasExamples ? ContentBase.Examples : Enumerable.Empty<XmlDocElement>();
        }

        /// <inheritdoc/>
        public bool HasPermissions {
            get { return IsContentBase && ContentBase.HasPermissions; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocRefElement> GetPermissions() {
            return HasPermissions ? ContentBase.Permissions : Enumerable.Empty<XmlDocRefElement>();
        }

        /// <inheritdoc/>
        public bool HasValueDescriptionContents {
            get { return IsValueMember && ValueMember.HasValueDescriptionContents; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GeValueDescriptionContents() {
            return HasValueDescriptionContents ? ValueMember.ValueDescriptionContents : Enumerable.Empty<XmlDocNode>();
        }

        /// <inheritdoc/>
        public bool HasRemarks {
            get { return IsContentBase && ContentBase.HasRemarks; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocElement> GetRemarks() {
            return HasRemarks ? ContentBase.Remarks : Enumerable.Empty<XmlDocElement>();
        }

        /// <inheritdoc/>
        public bool HasSeeAlso {
            get { return IsContentBase && ContentBase.HasSeeAlso; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocRefElement> GetSeeAlsos() {
            return HasSeeAlso ? ContentBase.SeeAlso : Enumerable.Empty<XmlDocRefElement>();
        }

        /// <inheritdoc/>
        public bool? IsPure {
            get { return IsInvokable ? Invokable.IsPure : null; }
        }

        /// <inheritdoc/>
        public ExternalVisibilityKind? ExternalVisibility {
            get { return Core.ExternalVisibility; }
        }

        /// <inheritdoc/>
        public bool? IsStatic {
            get { return IsContentBase ? ContentBase.IsStatic : null; }
        }

        /// <inheritdoc/>
        public bool? IsObsolete {
            get { return IsContentBase ? ContentBase.IsObsolete : null; }
        }

        private CodeDocParameter GetParameterByName(string parameterName) {
            return IsInvokable && Invokable.HasParameters
                ? Invokable.Parameters.FirstOrDefault(p => p.Name == parameterName)
                : null;
        }

        /// <inheritdoc/>
        public bool HasParameterSummaryContents(string parameterName) {
            var parameter = GetParameterByName(parameterName);
            return parameter != null && parameter.HasSummaryContents;
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GetParameterSummaryContents(string parameterName) {
            var parameter = GetParameterByName(parameterName);
            return parameter != null && parameter.HasSummaryContents
                ? parameter.SummaryContents
                : Enumerable.Empty<XmlDocNode>();
        }

        private CodeDocParameter GetReturnParameter() {
            return IsInvokable && Invokable.HasReturn ? Invokable.Return : null;
        }

        /// <inheritdoc/>
        public bool HasReturnSummaryContents {
            get {
                var returnParameter = GetReturnParameter();
                return returnParameter != null && returnParameter.HasSummaryContents;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GetReturnSummaryContents() {
            var returnParameter = GetReturnParameter();
            return returnParameter != null && returnParameter.HasSummaryContents
                ? returnParameter.SummaryContents
                : Enumerable.Empty<XmlDocNode>();
        }

        private CodeDocGenericParameter GetGenericParameter(string typeParameterName) {
            if (IsType)
                return Type.HasGenericParameters ? Type.GenericParameters.FirstOrDefault(p => p.Name == typeParameterName) : null;
            if (IsMethod)
                return Method.HasGenericParameters ? Method.GenericParameters.FirstOrDefault(p => p.Name == typeParameterName) : null;
            return null;
        }

        /// <inheritdoc/>
        public bool HasGenericTypeSummaryContents(string typeParameterName) {
            var parameter = GetGenericParameter(typeParameterName);
            return parameter != null && parameter.HasSummaryContents;
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocNode> GetGenericTypeSummaryContents(string typeParameterName) {
            var parameter = GetGenericParameter(typeParameterName);
            return parameter != null && parameter.HasSummaryContents
                ? parameter.SummaryContents
                : Enumerable.Empty<XmlDocNode>();
        }

        /// <inheritdoc/>
        public bool? RequiresParameterNotEverNull(string parameterName) {
            var parameter = GetParameterByName(parameterName);
            return parameter == null ? null : parameter.NullRestricted;
        }

        /// <inheritdoc/>
        public bool? EnsuresResultNotEverNull {
            get {
                var parameter = GetReturnParameter();
                return parameter == null ? null : parameter.NullRestricted;
            }
        }

        /// <inheritdoc/>
        public bool HasExceptions {
            get { return false; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocRefElement> GetExceptions() {
            return Enumerable.Empty<XmlDocRefElement>();
        }

        /// <inheritdoc/>
        public bool HasEnsures {
            get { return false; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocContractElement> GetEnsures() {
            return Enumerable.Empty<XmlDocContractElement>();
        }

        /// <inheritdoc/>
        public bool HasRequires {
            get { return false; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocContractElement> GetRequires() {
            return Enumerable.Empty<XmlDocContractElement>();
        }

        /// <inheritdoc/>
        public bool HasInvariants {
            get { return false; }
        }

        /// <inheritdoc/>
        public IEnumerable<XmlDocContractElement> GetInvariants() {
            return Enumerable.Empty<XmlDocContractElement>();
        }

    }
}
