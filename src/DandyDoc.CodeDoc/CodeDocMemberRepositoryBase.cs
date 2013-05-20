using System;
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

        public abstract ICodeDocMember GetMemberModel(CRefIdentifier cRef);

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


    }
}
