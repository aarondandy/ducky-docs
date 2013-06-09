using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public class CodeDocRepositorySearchContext
    {

        public CodeDocRepositorySearchContext(IEnumerable<ICodeDocMemberRepository> allRepositories)
            : this(allRepositories.ToArray()){
            Contract.Requires(allRepositories != null);
        }

        private CodeDocRepositorySearchContext(ICodeDocMemberRepository[] allRepositories) {
            Contract.Requires(allRepositories != null);
            _visitedRepositories = new HashSet<ICodeDocMemberRepository>();
            _allRepositories = allRepositories.ToArray();
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_visitedRepositories != null);
            Contract.Invariant(_allRepositories != null);
        }

        private readonly HashSet<ICodeDocMemberRepository> _visitedRepositories;
        private readonly ICodeDocMemberRepository[] _allRepositories;

        [Pure]
        public bool IsReferenced(ICodeDocMemberRepository repository) {
            return _allRepositories.Contains(repository);
        }

        public ReadOnlyCollection<ICodeDocMemberRepository> AllRepositories {
            get {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<ICodeDocMemberRepository>>() != null);
                return new ReadOnlyCollection<ICodeDocMemberRepository>(_allRepositories);
            }
        }

        public bool Visit(ICodeDocMemberRepository repository) {
            if(repository == null) throw new ArgumentNullException("repository");
            if(!IsReferenced(repository)) throw new ArgumentException("Given repository must be referenced by this search context.", "repository");
            Contract.EndContractBlock();
            return _visitedRepositories.Add(repository);
        }

        public IEnumerable<ICodeDocMemberRepository> VisitedRepositories {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICodeDocMemberRepository>>() != null);
                return _visitedRepositories.ToArray();
            }
        }

        public IEnumerable<ICodeDocMemberRepository> UnvisitedRepositories {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICodeDocMemberRepository>>() != null);
                if(_visitedRepositories.Count == 0)
                    return new ReadOnlyCollection<ICodeDocMemberRepository>(_allRepositories);
                if (_visitedRepositories.Count == _allRepositories.Length)
                    return Enumerable.Empty<ICodeDocMemberRepository>(); // assumes all visited items exist in _allRepositories
                return _allRepositories.Where(r => !_visitedRepositories.Contains(r));
            }
        }

        public ICodeDocMemberRepository PopUnvisitedRepository(){
            var nextRepository = UnvisitedRepositories.FirstOrDefault();
            if (nextRepository != null)
                Visit(nextRepository);
            return nextRepository;
        }

        public CodeDocRepositorySearchContext CloneWithoutVisits() {
            return new CodeDocRepositorySearchContext(_allRepositories);
        }

        public CodeDocRepositorySearchContext CloneWithSingleVisit(ICodeDocMemberRepository repository) {
            if(repository == null) throw new ArgumentNullException("repository");
            Contract.EndContractBlock();
            var result = CloneWithoutVisits();
            result.Visit(repository);
            return result;
        }

        public ICodeDocMember Search(CRefIdentifier cRef) {
            ICodeDocMemberRepository repository;
            while((repository = PopUnvisitedRepository()) != null){
                var model = repository.GetMemberModel(cRef, this);
                if (model != null)
                    return model;
            }
            return null;
        }

    }
}
