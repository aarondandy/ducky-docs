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

        public CodeDocRepositorySearchContext(IEnumerable<ICodeDocMemberRepository> allRepositories, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full)
            : this(allRepositories.ToArray(), detailLevel){
            Contract.Requires(allRepositories != null);
        }

        private CodeDocRepositorySearchContext(ICodeDocMemberRepository[] allRepositories, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            Contract.Requires(allRepositories != null);
            _visitedRepositories = new HashSet<ICodeDocMemberRepository>();
            _allRepositories = allRepositories.ToArray();
            DetailLevel = detailLevel;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_visitedRepositories != null);
            Contract.Invariant(_allRepositories != null);
        }

        private readonly HashSet<ICodeDocMemberRepository> _visitedRepositories;
        private readonly ICodeDocMemberRepository[] _allRepositories;

        public CodeDocMemberDetailLevel DetailLevel { get; set; }

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
            Contract.EndContractBlock();
            if (!IsReferenced(repository))
                return false;
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
            return CloneWithoutVisits(DetailLevel);
        }

        public CodeDocRepositorySearchContext CloneWithoutVisits(CodeDocMemberDetailLevel detailLevel) {
            return new CodeDocRepositorySearchContext(_allRepositories, detailLevel);
        }

        public CodeDocRepositorySearchContext CloneWithSingleVisit(ICodeDocMemberRepository repository) {
            return CloneWithSingleVisit(repository, DetailLevel);
        }

        public CodeDocRepositorySearchContext CloneWithSingleVisit(ICodeDocMemberRepository repository, CodeDocMemberDetailLevel detailLevel) {
            if(repository == null) throw new ArgumentNullException("repository");
            Contract.EndContractBlock();
            var result = CloneWithoutVisits(detailLevel);
            result.Visit(repository);
            return result;
        }

        public ICodeDocMember Search(CRefIdentifier cRef) {
            ICodeDocMemberRepository repository;
            while((repository = PopUnvisitedRepository()) != null){
                var model = repository.GetMemberModel(cRef, this, DetailLevel);
                if (model != null)
                    return model;
            }
            return null;
        }

        public CodeDocRepositorySearchContext CloneWithOneUnvisited(ICodeDocMemberRepository targetRepository) {
            return CloneWithOneUnvisited(targetRepository, DetailLevel);
        }

        public CodeDocRepositorySearchContext CloneWithOneUnvisited(ICodeDocMemberRepository targetRepository, CodeDocMemberDetailLevel detailLevel) {
            var result = CloneWithoutVisits(detailLevel);
            foreach (var repository in result.AllRepositories)
                if (repository != targetRepository)
                    result.Visit(repository);
            return result;
        }
    }
}
