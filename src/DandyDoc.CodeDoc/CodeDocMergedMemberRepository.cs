using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public class CodeDocMergedMemberRepository :
        Collection<ICodeDocMemberRepository>,
        ICodeDocMemberRepository
    {

        protected class MergedAssembliesAndNamespaces
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

        public CodeDocMergedMemberRepository() : this(null) { }

        public CodeDocMergedMemberRepository(IEnumerable<ICodeDocMemberRepository> repositories)
            : base(repositories == null ? new List<ICodeDocMemberRepository>() : repositories.ToList()) {
            ClearAssemblyNamespaceCache();
        }

        public CodeDocMergedMemberRepository(params ICodeDocMemberRepository[] repositories)
            : this((IEnumerable<ICodeDocMemberRepository>)repositories) { }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_assembliesAndNamespaces != null);
        }

        public ICodeDocMember GetMemberModel(CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext = null, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            return this.Select(r => r.GetMemberModel(cRef, searchContext, detailLevel)).FirstOrDefault(m => m != null);
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

        protected override void ClearItems() {
            base.ClearItems();
            ClearAssemblyNamespaceCache();
        }

        protected override void InsertItem(int index, ICodeDocMemberRepository item) {
            base.InsertItem(index, item);
            ClearAssemblyNamespaceCache();
        }

        protected override void RemoveItem(int index) {
            base.RemoveItem(index);
            ClearAssemblyNamespaceCache();
        }

        protected override void SetItem(int index, ICodeDocMemberRepository item) {
            base.SetItem(index, item);
            ClearAssemblyNamespaceCache();
        }

        public IList<CodeDocSimpleAssembly> Assemblies {
            get { return _assembliesAndNamespaces.Value.Assemblies; }
        }

        public IList<CodeDocSimpleNamespace> Namespaces {
            get { return _assembliesAndNamespaces.Value.Namespaces; }
        }
    }
}
