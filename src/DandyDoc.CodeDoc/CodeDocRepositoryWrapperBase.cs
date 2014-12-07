using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// A repository wrapper that can be used to extend functionality for a repository.
    /// </summary>
    public abstract class CodeDocRepositoryWrapperBase : ICodeDocMemberRepository
    {

        /// <summary>
        /// Creates a new repository wrapper for another <paramref name="repository"/>.
        /// </summary>
        /// <param name="repository">The repository to wrap.</param>
        protected CodeDocRepositoryWrapperBase(ICodeDocMemberRepository repository) {
            if (repository == null) throw new ArgumentNullException("repository");
            Contract.EndContractBlock();
            Repository = repository;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Repository != null);
        }

        /// <summary>
        /// The wrapped repository.
        /// </summary>
        protected ICodeDocMemberRepository Repository { get; private set; }

        /// <inheritdoc/>
        public abstract ICodeDocMember GetMemberModel(CRef.CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext = null, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full);

        /// <inheritdoc/>
        public abstract IList<CodeDocSimpleAssembly> Assemblies { get; }

        /// <inheritdoc/>
        public abstract IList<CodeDocSimpleNamespace> Namespaces { get; }

    }
}
