using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DuckyDocs.CRef;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// Merges multiple repositories into one single repository.
    /// </summary>
    public class CodeDocMergedMemberRepository :
        Collection<ICodeDocMemberRepository>,
        ICodeDocMemberRepository
    {

        private class MergedAssembliesAndNamespaces
        {

            public MergedAssembliesAndNamespaces(IEnumerable<CodeDocSimpleAssembly> assemblies, IEnumerable<CodeDocSimpleNamespace> namespaces) {
                if(assemblies == null) throw new ArgumentNullException("assemblies");
                if(namespaces == null) throw new ArgumentNullException("namespaces");
                Contract.EndContractBlock();
                Assemblies = new ReadOnlyCollection<CodeDocSimpleAssembly>(assemblies.ToArray());
                Namespaces = new ReadOnlyCollection<CodeDocSimpleNamespace>(namespaces.ToArray());
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(Assemblies != null);
                Contract.Invariant(Namespaces != null);
            }

            public ReadOnlyCollection<CodeDocSimpleAssembly> Assemblies { get; private set; }

            public ReadOnlyCollection<CodeDocSimpleNamespace> Namespaces { get; private set; }
        }

        private Lazy<MergedAssembliesAndNamespaces> _assembliesAndNamespaces;

        /// <summary>
        /// Creates a default empty merged repository.
        /// </summary>
        public CodeDocMergedMemberRepository() : this(null) { }

        /// <summary>
        /// Creates a merged repository from the given repositories.
        /// </summary>
        /// <param name="repositories">The initial repositories to merge.</param>
        public CodeDocMergedMemberRepository(IEnumerable<ICodeDocMemberRepository> repositories)
            : base(repositories == null ? new List<ICodeDocMemberRepository>() : repositories.ToList()) {
            ClearAssemblyNamespaceCache();
        }

        /// <summary>
        /// Creates a merged repository from the given repositories.
        /// </summary>
        /// <param name="repositories">The initial repositories to merge.</param>
        public CodeDocMergedMemberRepository(params ICodeDocMemberRepository[] repositories)
            : this((IEnumerable<ICodeDocMemberRepository>)repositories) { }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_assembliesAndNamespaces != null);
        }

        /// <inheritdoc/>
        public ICodeDocMember GetMemberModel(CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext = null, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            foreach (var subRepo in this) {
                var subResult = subRepo.GetMemberModel(cRef, searchContext, detailLevel);
                if (subResult != null)
                    return subResult;
            }
            return null;
        }

        private void ClearAssemblyNamespaceCache() {
            _assembliesAndNamespaces = new Lazy<MergedAssembliesAndNamespaces>(CreateMergedAssembliesAndNamespaces, true);
        }

        private MergedAssembliesAndNamespaces CreateMergedAssembliesAndNamespaces() {
            var assemblyLookup = new Dictionary<CRefIdentifier, CodeDocSimpleAssembly>();
            var namespaceLookup = new Dictionary<CRefIdentifier, CodeDocSimpleNamespace>();

            foreach (var repository in this) {
                foreach (var repositoryAssembly in repository.Assemblies) {
                    var cRef = repositoryAssembly.CRef;
                    CodeDocSimpleAssembly mergedAssembly;
                    if (!assemblyLookup.TryGetValue(cRef, out mergedAssembly)) {
                        mergedAssembly = new CodeDocSimpleAssembly(cRef);
                        mergedAssembly.AssemblyFileName = repositoryAssembly.AssemblyFileName;
                        assemblyLookup.Add(cRef, mergedAssembly);
                    }

                    foreach (var typeCRef in repositoryAssembly.TypeCRefs)
                        mergedAssembly.TypeCRefs.Add(typeCRef);

                    foreach(var namespaceCRef in repositoryAssembly.NamespaceCRefs)
                        if(!mergedAssembly.NamespaceCRefs.Contains(namespaceCRef))
                            mergedAssembly.NamespaceCRefs.Add(namespaceCRef);
                }

                foreach (var repositoryNamespace in repository.Namespaces) {
                    var cRef = repositoryNamespace.CRef;
                    CodeDocSimpleNamespace mergedNamespace;
                    if (!namespaceLookup.TryGetValue(cRef, out mergedNamespace)) {
                        mergedNamespace = new CodeDocSimpleNamespace(cRef);
                        namespaceLookup.Add(cRef, mergedNamespace);
                    }

                    foreach (var typeCRef in repositoryNamespace.TypeCRefs)
                        mergedNamespace.TypeCRefs.Add(typeCRef);

                    foreach(var assemblyCRef in repositoryNamespace.AssemblyCRefs)
                        if(!mergedNamespace.AssemblyCRefs.Contains(assemblyCRef))
                            mergedNamespace.AssemblyCRefs.Add(assemblyCRef);
                }
            }

            return new MergedAssembliesAndNamespaces(
                assemblyLookup.Values.OrderBy(x => x.CRef),
                namespaceLookup.Values.OrderBy(x => x.CRef));

        }

        /// <inheritdoc/>
        protected override void ClearItems() {
            base.ClearItems();
            ClearAssemblyNamespaceCache();
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, ICodeDocMemberRepository item) {
            base.InsertItem(index, item);
            ClearAssemblyNamespaceCache();
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index) {
            base.RemoveItem(index);
            ClearAssemblyNamespaceCache();
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, ICodeDocMemberRepository item) {
            base.SetItem(index, item);
            ClearAssemblyNamespaceCache();
        }

        /// <summary>
        /// All assemblies referenced by the merged repositories.
        /// </summary>
        public IList<CodeDocSimpleAssembly> Assemblies {
            get { return _assembliesAndNamespaces.Value.Assemblies; }
        }

        /// <summary>
        /// A collection of merged namespaces.
        /// </summary>
        public IList<CodeDocSimpleNamespace> Namespaces {
            get { return _assembliesAndNamespaces.Value.Namespaces; }
        }
    }
}
