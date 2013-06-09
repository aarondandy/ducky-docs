using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        protected class SimpleAssemblyNamespaceColleciton
        {

            public SimpleAssemblyNamespaceColleciton(IList<CodeDocSimpleAssembly> assemblies, IList<CodeDocSimpleNamespace> namespaces){
                if(assemblies == null) throw new ArgumentNullException("assemblies");
                if(namespaces == null) throw new ArgumentNullException("namespaces");
                Contract.EndContractBlock();
                Assemblies = new ReadOnlyCollection<CodeDocSimpleAssembly>(assemblies);
                Namespaces = new ReadOnlyCollection<CodeDocSimpleNamespace>(namespaces);
            }

            private void CodeContractInvariant(){
                Contract.Invariant(Assemblies != null);
                Contract.Invariant(Namespaces != null);
            }

            public ReadOnlyCollection<CodeDocSimpleAssembly> Assemblies { get; private set; }
            public ReadOnlyCollection<CodeDocSimpleNamespace> Namespaces { get; private set; }
        }

        private readonly Lazy<SimpleAssemblyNamespaceColleciton> _simpleAssemblyNamespaceCollection;

        /// <summary>
        /// A base constructor for code doc repositories.
        /// </summary>
        /// <param name="xmlDocs">The optional XML assembly documentation files.</param>
        protected CodeDocMemberRepositoryBase(IEnumerable<XmlAssemblyDocument> xmlDocs) {
            XmlDocs = new XmlAssemblyDocumentCollection(xmlDocs);
            _simpleAssemblyNamespaceCollection = new Lazy<SimpleAssemblyNamespaceColleciton>(CreateSimpleAssemblyNamespaceCollection, true);
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(XmlDocs != null);
            Contract.Invariant(_simpleAssemblyNamespaceCollection != null);
        }

        protected abstract SimpleAssemblyNamespaceColleciton CreateSimpleAssemblyNamespaceCollection();

        /// <summary>
        /// Assembly XML docs.
        /// </summary>
        public XmlAssemblyDocumentCollection XmlDocs { get; private set; }

        /// <inheritdoc/>
        public IList<CodeDocSimpleAssembly> Assemblies { get { return _simpleAssemblyNamespaceCollection.Value.Assemblies; } }

        /// <inheritdoc/>
        public IList<CodeDocSimpleNamespace> Namespaces { get { return _simpleAssemblyNamespaceCollection.Value.Namespaces; } }

        /// <inheritdoc/>
        public ICodeDocMember GetMemberModel(string cRef, CodeDocRepositorySearchContext searchContext = null) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Code reference is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetMemberModel(new CRefIdentifier(cRef), searchContext);
        }

        /// <inheritdoc/>
        public virtual ICodeDocMember GetMemberModel(CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext) {
            return GetMemberModel(cRef, searchContext, lite: false);
        }

        /// <summary>
        /// Creates a member model for the given code reference.
        /// </summary>
        /// <param name="cRef">The code reference to generate a model for.</param>
        /// <param name="lite">Indicates if a lite version of the model should be generated.</param>
        /// <returns>The generated member if possible.</returns>
        protected virtual ICodeDocMember GetMemberModel(CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext, bool lite) {
            return CreateGenerator(searchContext).GetMemberModel(cRef, lite: lite);
        }

        protected abstract MemberGeneratorBase CreateGenerator(CodeDocRepositorySearchContext searchContext);


        protected abstract class MemberGeneratorBase
        {

            protected MemberGeneratorBase(CodeDocMemberRepositoryBase repository, CodeDocRepositorySearchContext searchContext) {
                if(repository == null) throw new ArgumentNullException("repository");
                Contract.EndContractBlock();
                Repository = repository;
                SearchContext = searchContext;
            }

            private void CodeContractInvariants() {
                Contract.Invariant(Repository != null);
            }

            public CodeDocMemberRepositoryBase Repository { get; private set; }

            public CodeDocRepositorySearchContext SearchContext { get; private set; }

            public bool HasSearchContext {
                get {
                    Contract.Ensures(Contract.Result<bool>() == (SearchContext != null));
                    return SearchContext != null;
                }
            }

            public XmlAssemblyDocumentCollection XmlDocs {
                get {
                    Contract.Ensures(Contract.Result<XmlAssemblyDocumentCollection>() != null);
                    return Repository.XmlDocs;
                }
            }

            /// <summary>
            /// Creates a namespace from the given name.
            /// </summary>
            /// <param name="namespaceName">The name of the namespace to create.</param>
            /// <returns>A namespace model.</returns>
            private CodeDocSimpleMember CreateNamespaceFromName(string namespaceName) {
                Contract.Ensures(Contract.Result<CodeDocSimpleMember>() != null);
                string namespaceFriendlyName;
                if (String.IsNullOrWhiteSpace(namespaceName)) {
                    namespaceName = String.Empty;
                    namespaceFriendlyName = "global";
                }
                else {
                    namespaceFriendlyName = namespaceName;
                }
                var cRef = new CRefIdentifier("N:" + namespaceName);
                return new CodeDocSimpleMember(cRef) {
                    Uri = cRef.ToUri(),
                    FullName = namespaceFriendlyName,
                    ShortName = namespaceFriendlyName,
                    SubTitle = "Namespace",
                    Title = namespaceFriendlyName,
                    ExternalVisibility = ExternalVisibilityKind.Public
                };
            }

            private CodeDocSimpleMember GetOrCreateLocalNamespaceFromName(string namespaceName) {
                return Repository.Namespaces.FirstOrDefault(x => x.FullName == namespaceName)
                    ?? CreateNamespaceFromName(namespaceName);
            }

            /// <summary>
            /// Gets a namespace model by name or creates a new one if not found.
            /// </summary>
            /// <param name="namespaceName">The name of the namespace to find.</param>
            /// <returns>A namespace model matching the given name.</returns>
            protected CodeDocSimpleMember GetOrCreateNamespaceByName(string namespaceName) {
                Contract.Ensures(Contract.Result<CodeDocSimpleMember>() != null);
                // TODO: First try to get the namespace from a higher level repository so it can be merged
                return GetOrCreateLocalNamespaceFromName(namespaceName);
            }

            /// <summary>
            /// Gets a namespace model by code reference.
            /// </summary>
            /// <param name="cRef">The code reference.</param>
            /// <returns>A namespace model if found.</returns>
            protected CodeDocSimpleNamespace GetCodeDocSimpleNamespace(CRefIdentifier cRef) {
                Contract.Requires(cRef != null);
                return Repository.Namespaces.FirstOrDefault(x => cRef.Equals(x.CRef));
            }

            /// <inheritdoc/>
            public ICodeDocMember GetMemberModel(string cRef) {
                if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Code reference is not valid.", "cRef");
                Contract.EndContractBlock();
                return GetMemberModel(new CRefIdentifier(cRef));
            }

            /// <inheritdoc/>
            public virtual ICodeDocMember GetMemberModel(CRefIdentifier cRef) {
                return GetMemberModel(cRef, lite: false);
            }

            /// <summary>
            /// Creates a member model for the given code reference.
            /// </summary>
            /// <param name="cRef">The code reference to generate a model for.</param>
            /// <param name="lite">Indicates if a lite version of the model should be generated.</param>
            /// <returns>The generated member if possible.</returns>
            public abstract ICodeDocMember GetMemberModel(CRefIdentifier cRef, bool lite);

            /// <summary>
            /// Converts the simple namespace into a full namespace.
            /// </summary>
            /// <param name="simpleNamespace">The simple namespace to upgrade.</param>
            /// <returns>An full namespace.</returns>
            protected CodeDocNamespace ToFullNamespace(CodeDocSimpleNamespace simpleNamespace) {
                if (simpleNamespace == null) throw new ArgumentNullException("simpleNamespace");
                Contract.Ensures(Contract.Result<CodeDocNamespace>() != null);
                var result = new CodeDocNamespace(simpleNamespace.CRef) {
                    AssemblyCRefs = simpleNamespace.AssemblyCRefs,
                    TypeCRefs = simpleNamespace.TypeCRefs
                };
                result.Uri = simpleNamespace.Uri ?? simpleNamespace.CRef.ToUri();
                result.Assemblies = simpleNamespace.AssemblyCRefs.Select(GetCodeDocSimpleAssembly).ToList();
                result.Types = simpleNamespace.TypeCRefs.Select(GetMemberModel).Cast<CodeDocType>().ToList();
                return result;
            }

            /// <summary>
            /// Gets an assembly model by code reference.
            /// </summary>
            /// <param name="cRef">The code reference.</param>
            /// <returns>An assembly model if found.</returns>
            public CodeDocSimpleAssembly GetCodeDocSimpleAssembly(CRefIdentifier cRef) {
                Contract.Requires(cRef != null);
                var assemblies = Repository.Assemblies;
                return assemblies.FirstOrDefault(x => cRef.Equals(x.CRef))
                    ?? assemblies.FirstOrDefault(x => cRef.CoreName == x.AssemblyFileName)
                    ?? assemblies.FirstOrDefault(x => cRef.CoreName == x.ShortName);
            }

            /// <summary>
            /// Creates a code doc placeholder model for a given code reference.
            /// </summary>
            /// <param name="cRef">The code reference to construct a model for.</param>
            /// <returns>A code model representing the given code reference.</returns>
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
                    Uri = cRef.ToUri(),
                    ShortName = cRefFullName,
                    Title = cRefFullName,
                    SubTitle = subTitle,
                    FullName = cRefFullName,
                    NamespaceName = String.Empty,
                    ExternalVisibility = ExternalVisibilityKind.Public
                };
            }

            /// <summary>
            /// Applies XML doc attributes from a provider to a content model.
            /// </summary>
            /// <param name="model">The model to apply attributes to.</param>
            /// <param name="provider">The provider to get attributes from.</param>
            protected virtual void ApplyContentXmlDocs(CodeDocMemberContentBase model, ICodeDocMemberDataProvider provider) {
                Contract.Requires(model != null);
                Contract.Requires(provider != null);
                model.SummaryContents = provider.GetSummaryContents().ToArray();
                model.Examples = provider.GetExamples().ToArray();
                model.Permissions = provider.GetPermissions().ToArray();
                model.Remarks = provider.GetRemarks().ToArray();
                model.SeeAlso = provider.GetSeeAlsos().ToArray();
            }

            protected ICodeDocMember GetOnly(CRefIdentifier cRef, bool lite = false) {
                Contract.Requires(cRef != null);

                if(HasSearchContext){
                    var searchContext = SearchContext.CloneWithSingleVisit(Repository);
                    var searchResult = searchContext.Search(cRef);
                    if (searchResult != null)
                        return searchResult;
                }

                var localModel = GetMemberModel(cRef, lite: lite);
                if (localModel != null)
                    return localModel;

                return null;
            }

            protected CodeDocType GetOnlyType(CRefIdentifier cRef, bool lite = false) {
                Contract.Requires(cRef != null);
                return ToTypeModel(GetOnly(cRef, lite: lite));
            }

            /// <summary>
            /// Gets a model for the given code reference or creates a new one.
            /// </summary>
            /// <param name="cRef">The code reference to get a model for.</param>
            /// <param name="lite">Indicates that the model should be lite.</param>
            /// <returns>A code doc model for the given code reference.</returns>
            protected ICodeDocMember GetOrConvert(CRefIdentifier cRef, bool lite = false) {
                Contract.Requires(cRef != null);
                Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
                return GetOnly(cRef, lite: lite)
                    ?? CreateGeneralMemberPlaceholder(cRef);
            }

            /// <summary>
            /// Gets a type model for the given code reference or creates a new one.
            /// </summary>
            /// <param name="cRef">The code reference to get a model for.</param>
            /// <param name="lite">Indicates that the model should be lite.</param>
            /// <returns>A code doc model for the given code reference.</returns>
            protected CodeDocType GetOrConvertType(CRefIdentifier cRef, bool lite = false) {
                Contract.Requires(cRef != null);
                Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
                return ToTypeModel(GetOrConvert(cRef, lite));
            }

            protected CodeDocType ToTypeModel(ICodeDocMember member){
                if (member == null)
                    return null;
                if (member is CodeDocType)
                    return (CodeDocType)member;

                var typeModel = new CodeDocType(member.CRef.WithTargetType("T"));

                var simpleMember = member as CodeDocSimpleMember;
                if(simpleMember != null){
                    typeModel.Uri = simpleMember.Uri;
                    typeModel.Title = simpleMember.Title;
                    typeModel.SubTitle = simpleMember.SubTitle;
                    typeModel.ShortName = simpleMember.ShortName;
                    typeModel.FullName = simpleMember.FullName;
                    typeModel.NamespaceName = simpleMember.NamespaceName;
                    typeModel.Namespace = simpleMember.Namespace;
                    typeModel.ExternalVisibility = simpleMember.ExternalVisibility;
                    typeModel.SummaryContents = simpleMember.SummaryContents;
                }

                return typeModel;
            }

            /// <summary>
            /// Creates exception models from a collection of XML doc exception elements.
            /// </summary>
            /// <param name="exceptionElement">The exception elements to convert to exception models.</param>
            /// <returns>A collection of exception models.</returns>
            /// <remarks>
            /// The given XML doc exception elements could be merged together when converted to exception models.
            /// This allows aggregation of exceptions by exception type rather than offering a flat list of exception conditions.
            /// This also enable the association with code contract ensures with exceptions.
            /// </remarks>
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
                        exceptionModel = new CodeDocException(GetOrConvertType(exceptionCRef, lite: true));
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
}
