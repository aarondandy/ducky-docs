using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public class ThreadSafeCodeDocRepositoryWrapper : ICodeDocEntityRepository
    {

        public ThreadSafeCodeDocRepositoryWrapper(ICodeDocEntityRepository repository) {
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

        protected ICodeDocEntityRepository Repository { get; private set; }

        private readonly object _mutex;

        public ICodeDocEntityContent GetContentEntity(string cRef) {
            lock (_mutex) {
                return Repository.GetContentEntity(cRef);
            }
        }

        public ICodeDocEntityContent GetContentEntity(CRefIdentifier cRef) {
            lock (_mutex) {
                return Repository.GetContentEntity(cRef);
            }
        }

        public ICodeDocEntity GetSimpleEntity(string cRef) {
            lock (_mutex) {
                return Repository.GetSimpleEntity(cRef);
            }
        }

        public ICodeDocEntity GetSimpleEntity(CRefIdentifier cRef) {
            lock (_mutex) {
                return Repository.GetSimpleEntity(cRef);
            }
        }

        public IList<ICodeDocAssembly> Assemblies {
            get {
                lock (_mutex) {
                    return Repository.Assemblies;
                }
            }
        }

        public IList<ICodeDocNamespace> Namespaces {
            get {
                lock (_mutex) {
                    return Repository.Namespaces;
                }
            }
        }
    }
}
