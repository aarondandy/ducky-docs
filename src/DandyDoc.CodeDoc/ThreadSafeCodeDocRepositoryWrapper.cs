﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A locking code doc repository wrapper.
    /// </summary>
    public class ThreadSafeCodeDocRepositoryWrapper : ICodeDocMemberRepository
    {

        /// <summary>
        /// Creates a new locking wrapper for another repository.
        /// </summary>
        /// <param name="repository">The repository to wrap.</param>
        public ThreadSafeCodeDocRepositoryWrapper(ICodeDocMemberRepository repository) {
            if(repository == null) throw new ArgumentNullException("repository");
            Contract.EndContractBlock();
            Repository = repository;
            _mutex = new object();
        }

        [ContractInvariantMethod]
        private void CodeContractInvaraints() {
            Contract.Invariant(Repository != null);
            Contract.Invariant(_mutex != null);
        }

        /// <summary>
        /// The wrapped repository.
        /// </summary>
        protected ICodeDocMemberRepository Repository { get; private set; }

        private readonly object _mutex;

        /// <summary>
        /// A locked request to the wrapped repository for a member model.
        /// </summary>
        /// <param name="cRef">The code reference.</param>
        /// <returns>The member model.</returns>
        public ICodeDocMember GetMemberModel(string cRef) {
            lock (_mutex) {
                return Repository.GetMemberModel(cRef);
            }
        }

        /// <summary>
        /// A locked request to the wrapped repository for a member model.
        /// </summary>
        /// <param name="cRef">The code reference.</param>
        /// <returns>The member model.</returns>
        public ICodeDocMember GetMemberModel(CRefIdentifier cRef) {
            lock (_mutex) {
                return Repository.GetMemberModel(cRef);
            }
        }

        /// <summary>
        /// A locked request to the wrapped repository for assemblies.
        /// </summary>
        public IList<CodeDocSimpleAssembly> Assemblies {
            get {
                lock (_mutex) {
                    return Repository.Assemblies;
                }
            }
        }

        /// <summary>
        /// A locked request to the wrapped repository for namespaces.
        /// </summary>
        public IList<CodeDocSimpleNamespace> Namespaces {
            get {
                lock (_mutex) {
                    return Repository.Namespaces;
                }
            }
        }

    }
}
