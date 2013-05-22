﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.ExternalVisibility;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A base code doc repository intended for the reflection and Cecil repositories.
    /// </summary>
    public abstract class CodeDocMemberRepositoryBase : ICodeDocMemberRepository
    {

        /// <summary>
        /// A base constructor for code doc repositories.
        /// </summary>
        /// <param name="xmlDocs">The optional XML assembly documentation files.</param>
        protected CodeDocMemberRepositoryBase(IEnumerable<XmlAssemblyDocument> xmlDocs) {
            XmlDocs = new XmlAssemblyDocumentCollection(xmlDocs);
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(XmlDocs != null);
        }

        /// <summary>
        /// Assembly XML docs.
        /// </summary>
        public XmlAssemblyDocumentCollection XmlDocs { get; private set; }

        /// <inheritdoc/>
        public IList<CodeDocSimpleAssembly> Assemblies { get; protected set; }

        /// <inheritdoc/>
        public IList<CodeDocSimpleNamespace> Namespaces { get; protected set; }

        /// <inheritdoc/>
        [Obsolete]
        public virtual ICodeDocMember GetContentMember(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Code reference is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetContentMember(new CRefIdentifier(cRef));
        }

        /// <inheritdoc/>
        [Obsolete]
        public abstract ICodeDocMember GetContentMember(CRefIdentifier cRef);

        /// <inheritdoc/>
        [Obsolete]
        public ICodeDocMember GetSimpleMember(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Code reference is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetSimpleMember(new CRefIdentifier(cRef));
        }

        /// <inheritdoc/>
        [Obsolete]
        public abstract ICodeDocMember GetSimpleMember(CRefIdentifier cRef);

        public ICodeDocMember GetMemberModel(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Code reference is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetMemberModel(new CRefIdentifier(cRef));
        }

        public virtual ICodeDocMember GetMemberModel(CRefIdentifier cRef) {
            return GetMemberModel(cRef, lite: false);
        }

        protected abstract ICodeDocMember GetMemberModel(CRefIdentifier cRef, bool lite);

        /// <summary>
        /// Gets a namespace model by code reference.
        /// </summary>
        /// <param name="cRef">The code reference.</param>
        /// <returns>A namespace model if found.</returns>
        protected CodeDocSimpleNamespace GetCodeDocSimpleNamespace(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            return Namespaces.FirstOrDefault(x => cRef.Equals(x.CRef));
        }

        /// <summary>
        /// Converts the simple namespace into a full namespace.
        /// </summary>
        /// <param name="simpleNamespace">The simple namespace to upgrade.</param>
        /// <returns>An full namespace.</returns>
        protected CodeDocNamespace ToFullNamespace(CodeDocSimpleNamespace simpleNamespace) {
            if(simpleNamespace == null) throw new ArgumentNullException("simpleNamespace");
            Contract.Ensures(Contract.Result<CodeDocNamespace>() != null);
            var result = new CodeDocNamespace(simpleNamespace.CRef) {
                AssemblyCRefs = simpleNamespace.AssemblyCRefs,
                TypeCRefs = simpleNamespace.TypeCRefs
            };
            result.Assemblies = simpleNamespace.AssemblyCRefs.Select(GetCodeDocSimpleAssembly).ToList();
            result.Types = simpleNamespace.TypeCRefs.Select(GetContentMember).Cast<CodeDocType>().ToList();
            return result;
        }

        /// <summary>
        /// Gets a namespace model by name or creates a new one if not found.
        /// </summary>
        /// <param name="namespaceName">The name of the namespace to find.</param>
        /// <returns>A namespace model matching the given name.</returns>
        [Obsolete("This should be a final effort, best to get the namespace from the top of a search tree so it can be merged with other repositories covering the same namespaces.")]
        protected CodeDocSimpleMember GetCodeDocNamespaceByName(string namespaceName) {
            Contract.Ensures(Contract.Result<CodeDocSimpleMember>() != null);
            return Namespaces.FirstOrDefault(x => x.FullName == namespaceName)
                ?? CreateNamespaceFromName(namespaceName);
        }

        /// <summary>
        /// Creates a namespace from the given name.
        /// </summary>
        /// <param name="namespaceName">The name of the namespace to create.</param>
        /// <returns>A namespace model.</returns>
        protected CodeDocSimpleMember CreateNamespaceFromName(string namespaceName) {
            Contract.Ensures(Contract.Result<CodeDocSimpleMember>() != null);
            string namespaceFriendlyName;
            if (String.IsNullOrWhiteSpace(namespaceName)) {
                namespaceName = String.Empty;
                namespaceFriendlyName = "global";
            }
            else {
                namespaceFriendlyName = namespaceName;
            }
            return new CodeDocSimpleMember(new CRefIdentifier("N:" + namespaceName)) {
                FullName = namespaceFriendlyName,
                ShortName = namespaceFriendlyName,
                SubTitle = "Namespace",
                Title = namespaceFriendlyName,
                ExternalVisibility = ExternalVisibilityKind.Public
            };
        }

        /// <summary>
        /// Gets an assembly model by code reference.
        /// </summary>
        /// <param name="cRef">The code reference.</param>
        /// <returns>An assembly model if found.</returns>
        protected CodeDocSimpleAssembly GetCodeDocSimpleAssembly(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            return Assemblies.FirstOrDefault(x => cRef.Equals(x.CRef))
                ?? Assemblies.FirstOrDefault(x => cRef.CoreName == x.AssemblyFileName)
                ?? Assemblies.FirstOrDefault(x => cRef.CoreName == x.ShortName);
        }

        /// <summary>
        /// Creates a simple type model placeholder for a code reference.
        /// </summary>
        /// <param name="cRef">The code reference.</param>
        /// <returns>A new simple model for a code reference.</returns>
        [Obsolete]
        protected ICodeDocMember CreateTypeMemberPlaceholder(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
            var cRefFullName = cRef.CoreName;
            return new CodeDocSimpleMember(cRef) {
                ShortName = cRefFullName,
                Title = cRefFullName,
                SubTitle = "Type",
                FullName = cRefFullName,
                NamespaceName = String.Empty,
                ExternalVisibility = ExternalVisibilityKind.Public
            };
        }

        protected ICodeDocMember CreateGeneralMemberPlaceholder(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);

            string subTitle;
            if ("T".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                subTitle = "Type";
            else if ("M".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                subTitle = "Invokable";
            else if ("P".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                subTitle = "Property";
            else if ("E".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                subTitle = "Event";
            else if ("F".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                subTitle = "Field";
            else if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                subTitle = "Namespace";
            else if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                subTitle = "Assembly";
            else
                subTitle = String.Empty;

            var cRefFullName = cRef.CoreName;
            return new CodeDocSimpleMember(cRef) {
                ShortName = cRefFullName,
                Title = cRefFullName,
                SubTitle = subTitle,
                FullName = cRefFullName,
                NamespaceName = String.Empty,
                ExternalVisibility = ExternalVisibilityKind.Public
            };
        }

        protected virtual void ApplyContentXmlDocs(CodeDocMemberContentBase model, ICodeDocMemberDataProvider provider) {
            Contract.Requires(model != null);
            Contract.Requires(provider != null);
            model.SummaryContents = provider.GetSummaryContents().ToArray();
            model.Examples = provider.GetExamples().ToArray();
            model.Permissions = provider.GetPermissions().ToArray();
            model.Remarks = provider.GetRemarks().ToArray();
            model.SeeAlso = provider.GetSeeAlsos().ToArray();
        }

        protected ICodeDocMember GetOrConvert(CRefIdentifier cRef, bool lite = false) {
            Contract.Requires(cRef != null);
            // TODO:
            // 1) Use the repository tree to get the model by cRef (so we can use a cache)
            // 1a) Make sure to include this repostitory in the search, but last (should be behind a cache)
            // 2) Try to make a model for it

            // TODO: need to look elsewhere for a model.
            var localModel = GetMemberModel(cRef, lite: lite);
            if (localModel != null)
                return localModel;

            return CreateGeneralMemberPlaceholder(cRef);
        }

        protected IEnumerable<CodeDocException> CreateExceptionModels(IEnumerable<XmlDocRefElement> exceptionElement) {
            Contract.Requires(exceptionElement != null);
            Contract.Ensures(Contract.Result<IEnumerable<CodeDocException>>() != null);
            var exceptionLookup = new Dictionary<CRefIdentifier, CodeDocException>();
            foreach (var xmlDocException in exceptionElement) {
                var exceptionCRef = String.IsNullOrWhiteSpace(xmlDocException.CRef)
                    ? new CRefIdentifier("T:")
                    : new CRefIdentifier(xmlDocException.CRef);
                CodeDocException exceptionModel;
                if (!exceptionLookup.TryGetValue(exceptionCRef, out exceptionModel)) {
                    exceptionModel = new CodeDocException(GetOrConvert(exceptionCRef, lite: true));
                    exceptionModel.Ensures = new List<XmlDocNode>();
                    exceptionModel.Conditions = new List<XmlDocNode>();
                    exceptionLookup.Add(exceptionCRef, exceptionModel);
                }
                var priorElement = xmlDocException.PriorElement;
                var isRelatedEnsuresOnThrow = null != priorElement
                    && String.Equals("ensuresOnThrow", priorElement.Name, StringComparison.OrdinalIgnoreCase)
                    && priorElement.Element.GetAttribute("exception") == exceptionCRef.FullCRef;
                if (isRelatedEnsuresOnThrow) {
                    if (priorElement.HasChildren) {
                        exceptionModel.Ensures.Add(priorElement);
                    }
                }
                else {
                    if (xmlDocException.HasChildren) {
                        exceptionModel.Conditions.Add(xmlDocException);
                    }
                }
            }
            return exceptionLookup.Values;
        }

    }
}
