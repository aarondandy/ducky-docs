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
            : this(new ReadOnlyCollection<ICodeDocMemberRepository>(allRepositories.ToArray()), detailLevel){
            Contract.Requires(allRepositories != null);
        }

        public CodeDocRepositorySearchContext(ICodeDocMemberRepository repository, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full)
            : this(new ReadOnlyCollection<ICodeDocMemberRepository>(new[] { repository }), detailLevel) {
            if(repository == null) throw new ArgumentNullException("repository");
            Contract.EndContractBlock();
        }

        private CodeDocRepositorySearchContext(ReadOnlyCollection<ICodeDocMemberRepository> allRepositories, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            Contract.Requires(allRepositories != null);
            _visitedRepositories = new HashSet<ICodeDocMemberRepository>();
            AllRepositories = allRepositories;
            DetailLevel = detailLevel;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_visitedRepositories != null);
            Contract.Invariant(AllRepositories != null);
        }

        private readonly HashSet<ICodeDocMemberRepository> _visitedRepositories;

        public CodeDocMemberDetailLevel DetailLevel { get; set; }

        [Pure]
        public bool IsReferenced(ICodeDocMemberRepository repository) {
            return AllRepositories.Contains(repository);
        }

        public ReadOnlyCollection<ICodeDocMemberRepository> AllRepositories { get; private set; }

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
                    return new ReadOnlyCollection<ICodeDocMemberRepository>(AllRepositories);
                return AllRepositories.Where(r => !_visitedRepositories.Contains(r));
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
            return new CodeDocRepositorySearchContext(AllRepositories, detailLevel);
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
