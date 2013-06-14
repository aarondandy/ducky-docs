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

        public CodeDocRepositorySearchContext(IEnumerable<ICodeDocMemberRepository> allRepositories, bool liteModels = false)
            : this(allRepositories.ToArray(), liteModels){
            Contract.Requires(allRepositories != null);
        }

        private CodeDocRepositorySearchContext(ICodeDocMemberRepository[] allRepositories, bool liteModels) {
            Contract.Requires(allRepositories != null);
            _visitedRepositories = new HashSet<ICodeDocMemberRepository>();
            _allRepositories = allRepositories.ToArray();
            LiteModels = liteModels;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_visitedRepositories != null);
            Contract.Invariant(_allRepositories != null);
        }

        private readonly HashSet<ICodeDocMemberRepository> _visitedRepositories;
        private readonly ICodeDocMemberRepository[] _allRepositories;

        public bool LiteModels { get; set; }

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
            return CloneWithoutVisits(LiteModels);
        }

        public CodeDocRepositorySearchContext CloneWithoutVisits(bool liteModels) {
            return new CodeDocRepositorySearchContext(_allRepositories, liteModels);
        }

        public CodeDocRepositorySearchContext CloneWithSingleVisit(ICodeDocMemberRepository repository) {
            return CloneWithSingleVisit(repository, LiteModels);
        }

        public CodeDocRepositorySearchContext CloneWithSingleVisit(ICodeDocMemberRepository repository, bool liteModels) {
            if(repository == null) throw new ArgumentNullException("repository");
            Contract.EndContractBlock();
            var result = CloneWithoutVisits(liteModels);
            result.Visit(repository);
            return result;
        }

        public ICodeDocMember Search(CRefIdentifier cRef) {
            ICodeDocMemberRepository repository;
            while((repository = PopUnvisitedRepository()) != null){
                var model = repository.GetMemberModel(cRef, this, lite: LiteModels);
                if (model != null)
                    return model;
            }
            return null;
        }

        public CodeDocRepositorySearchContext CloneWithOneUnvisited(ICodeDocMemberRepository targetRepository) {
            return CloneWithOneUnvisited(targetRepository, LiteModels);
        }

        public CodeDocRepositorySearchContext CloneWithOneUnvisited(ICodeDocMemberRepository targetRepository, bool liteModels) {
            var result = CloneWithoutVisits(liteModels);
            foreach (var repository in result.AllRepositories)
                if (repository != targetRepository)
                    result.Visit(repository);
            return result;
        }
    }
}
