using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DuckyDocs.CRef;

namespace DuckyDocs.CodeDoc
{
    /// <summary>
    /// A locking code doc repository wrapper.
    /// </summary>
    public class ThreadSafeCodeDocRepositoryWrapper : CodeDocRepositoryWrapperBase
    {

        /// <summary>
        /// Creates a new locking wrapper for another repository.
        /// </summary>
        /// <param name="repository">The repository to wrap.</param>
        public ThreadSafeCodeDocRepositoryWrapper(ICodeDocMemberRepository repository) : base(repository)
        {
            Contract.Requires(repository != null);
            _mutex = new object();
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_mutex != null);
        }

        private readonly object _mutex;

        /// <summary>
        /// A locked request to the wrapped repository for a member model.
        /// </summary>
        /// <param name="cRef">The code reference.</param>
        /// <param name="searchContext">The search context to use when locating other models.</param>
        /// <param name="detailLevel">Indicates the desired detail level of the generated model.</param>
        /// <returns>The member model.</returns>
        public override ICodeDocMember GetMemberModel(CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext = null, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            lock (_mutex) {
                return Repository.GetMemberModel(cRef, searchContext, detailLevel);
            }
        }

        /// <summary>
        /// A locked request to the wrapped repository for assemblies.
        /// </summary>
        public override IList<CodeDocSimpleAssembly> Assemblies {
            get {
                lock (_mutex) {
                    return Repository.Assemblies;
                }
            }
        }

        /// <summary>
        /// A locked request to the wrapped repository for namespaces.
        /// </summary>
        public override IList<CodeDocSimpleNamespace> Namespaces {
            get {
                lock (_mutex) {
                    return Repository.Namespaces;
                }
            }
        }

    }
}
