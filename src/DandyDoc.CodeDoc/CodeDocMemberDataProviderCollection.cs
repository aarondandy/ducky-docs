using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CodeDoc.Utility;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocMemberDataProviderCollection :
        Collection<ICodeDocMemberDataProvider>,
        ICodeDocMemberDataProvider
    {

        

        public CodeDocMemberDataProviderCollection()
            : this(new List<ICodeDocMemberDataProvider>()) { }

        public CodeDocMemberDataProviderCollection(IEnumerable<ICodeDocMemberDataProvider> providers)
            : base(providers.ToList()) {
            Contract.Requires(providers != null);
            MergeGroups = false;
        }

        public virtual bool MergeGroups { get; private set; }

        public virtual bool HasExamples { get { return this.Any(x => x.HasExamples); } }

        public virtual IEnumerable<XmlDocElement> GetExamples() {
            var relevantProviders = this.Where(x => x.HasExamples);
            if (MergeGroups)
                return relevantProviders.SelectMany(x => x.GetExamples());
            return relevantProviders
                .Select(x => x.GetExamples())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocElement>();
        }

        public virtual bool HasPermissions { get { return this.Any(x => x.HasPermissions); } }

        public virtual IEnumerable<XmlDocRefElement> GetPermissions() {
            var relevantProviders = this.Where(x => x.HasPermissions);
            if (MergeGroups)
                return relevantProviders.SelectMany(x => x.GetPermissions());
            return relevantProviders
                .Select(x => x.GetPermissions())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocRefElement>();
        } 

        public virtual bool HasSummaryContents {
            get { return this.Any(x => x.HasSummaryContents); }
        }

        public virtual IEnumerable<XmlDocNode> GetSummaryContents() {
            return this.Where(x => x.HasSummaryContents)
                .Select(x => x.GetSummaryContents())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }


        public virtual bool HasValueDescriptionContents {
            get { return this.Any(x => x.HasValueDescriptionContents); }
        }

        public virtual IEnumerable<XmlDocNode> GeValueDescriptionContents() {
            return this.Where(x => x.HasValueDescriptionContents)
                .Select(x => x.GeValueDescriptionContents())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }

        public virtual bool HasRemarks {
            get { return this.Any(x => x.HasRemarks); }
        }

        public virtual IEnumerable<XmlDocElement> GetRemarks() {
            var relevantProviders = this.Where(x => x.HasRemarks);
            if (MergeGroups)
                return relevantProviders.SelectMany(x => x.GetRemarks());
            return relevantProviders
                .Select(x => x.GetRemarks())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocElement>();
        }

        public virtual bool HasSeeAlsos {
            get { return this.Any(x => x.HasSeeAlsos); }
        }

        public virtual IEnumerable<XmlDocRefElement> GetSeeAlsos() {
            var relevantProviders = this.Where(x => x.HasSeeAlsos);
            if (MergeGroups)
                return relevantProviders.SelectMany(x => x.GetSeeAlsos());
            return relevantProviders
                .Select(x => x.GetSeeAlsos())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocRefElement>();
        }

        public virtual bool? IsPure {
            get { return this.Select(x => x.IsPure).FirstSetNullableOrDefault(); }
        }

        public virtual ExternalVisibility.ExternalVisibilityKind? ExternalVisibility {
            get { return this.Select(x => x.ExternalVisibility).FirstSetNullableOrDefault(); }
        }

        public virtual bool? IsStatic {
            get { return this.Select(x => x.IsStatic).FirstSetNullableOrDefault(); }
        }

        public virtual bool? IsObsolete {
            get { return this.Select(x => x.IsObsolete).FirstSetNullableOrDefault(); }
        }

        public virtual bool HasParameterSummaryContents(string parameterName) {
            return this.Any(x => x.HasParameterSummaryContents(parameterName));
        }

        public virtual IEnumerable<XmlDocNode> GetParameterSummaryContents(string parameterName) {
            return this
                .Where(x => x.HasParameterSummaryContents(parameterName))
                .Select(x => x.GetParameterSummaryContents(parameterName))
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }

        public virtual bool HasReturnSummaryContents {
            get { return this.Any(x => x.HasReturnSummaryContents); }
        }

        public virtual IEnumerable<XmlDocNode> GetReturnSummaryContents() {
            return this
                .Where(x => x.HasReturnSummaryContents)
                .Select(x => x.GetReturnSummaryContents())
                .FirstOrDefault()
                ?? Enumerable.Empty<XmlDocNode>();
        }

        public virtual bool? RequiresParameterNotEverNull(string parameterName) {
            return this.Select(x => x.RequiresParameterNotEverNull(parameterName)).FirstSetNullableOrDefault();
        }


        public virtual bool? EnsuresResultNotEverNull {
            get {
                return this
                    .Select(x => x.EnsuresResultNotEverNull)
                    .FirstSetNullableOrDefault();
            }
        }

    }
}
