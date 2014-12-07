using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DuckyDocs.CRef;

namespace DuckyDocs.CodeDoc
{
    /// <summary>
    /// A search context used to recursively locate and generate code documentation member models.
    /// </summary>
    public class CodeDocRepositorySearchContext
    {

        /// <summary>
        /// Constructs a new search context with the given repositories and detail level.
        /// </summary>
        /// <param name="allRepositories">The repositories that are to be searched.</param>
        /// <param name="detailLevel">The desired detail level.</param>
        public CodeDocRepositorySearchContext(IEnumerable<ICodeDocMemberRepository> allRepositories, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full)
            : this(new ReadOnlyCollection<ICodeDocMemberRepository>(allRepositories.ToArray()), detailLevel){
            Contract.Requires(allRepositories != null);
        }

        /// <summary>
        /// Constructs a new search context with the given repositories and detail level.
        /// </summary>
        /// <param name="repository">The repository that is to be searched.</param>
        /// <param name="detailLevel">The desired detail level.</param>
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

        /// <summary>
        /// The desired detail level of generated models from this search context.
        /// </summary>
        public CodeDocMemberDetailLevel DetailLevel { get; set; }

        /// <summary>
        /// Determines if the given repository is directly referenced by this search context.
        /// </summary>
        /// <param name="repository">The repository to check for.</param>
        /// <returns><c>true</c> when the <paramref name="repository"/> is referenced.</returns>
        [Pure] public bool IsReferenced(ICodeDocMemberRepository repository) {
            return AllRepositories.Contains(repository);
        }

        /// <summary>
        /// All repositories referenced by this search context.
        /// </summary>
        public ReadOnlyCollection<ICodeDocMemberRepository> AllRepositories { get; private set; }

        /// <summary>
        /// Marks a repository as visited.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns><c>true</c> when the repository is marked as visited.</returns>
        public bool Visit(ICodeDocMemberRepository repository) {
            if(repository == null) throw new ArgumentNullException("repository");
            Contract.EndContractBlock();
            if (!IsReferenced(repository))
                return false;
            return _visitedRepositories.Add(repository);
        }

        /// <summary>
        /// The repositories that have been searched or that are currently not available to be searched.
        /// </summary>
        public IEnumerable<ICodeDocMemberRepository> VisitedRepositories {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICodeDocMemberRepository>>() != null);
                return _visitedRepositories.ToArray();
            }
        }

        /// <summary>
        /// The yet to be searched repositories.
        /// </summary>
        public IEnumerable<ICodeDocMemberRepository> UnvisitedRepositories {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICodeDocMemberRepository>>() != null);
                if(_visitedRepositories.Count == 0)
                    return new ReadOnlyCollection<ICodeDocMemberRepository>(AllRepositories);
                return AllRepositories.Where(r => !_visitedRepositories.Contains(r));
            }
        }

        /// <summary>
        /// Returns a repository located in the unvisited list and moves it to the visited list.
        /// </summary>
        /// <returns>A repository that has not yet been visited or <c>null</c>.</returns>
        public ICodeDocMemberRepository PopUnvisitedRepository(){
            var nextRepository = UnvisitedRepositories.FirstOrDefault();
            if (nextRepository != null)
                Visit(nextRepository);
            return nextRepository;
        }

        /// <summary>
        /// Clones this search context with no repositories visited.
        /// </summary>
        /// <returns>A search context.</returns>
        public CodeDocRepositorySearchContext CloneWithoutVisits() {
            Contract.Ensures(Contract.Result<CodeDocRepositorySearchContext>() != null);
            return CloneWithoutVisits(DetailLevel);
        }

        /// <summary>
        /// Clones this search context with no repositories visited and an overridden detail level.
        /// </summary>
        /// <param name="detailLevel">The desired detail level.</param>
        /// <returns>A search context.</returns>
        public CodeDocRepositorySearchContext CloneWithoutVisits(CodeDocMemberDetailLevel detailLevel) {
            Contract.Ensures(Contract.Result<CodeDocRepositorySearchContext>() != null);
            return new CodeDocRepositorySearchContext(AllRepositories, detailLevel);
        }

        /// <summary>
        /// Clones this search context with only the given repository marked as visited.
        /// </summary>
        /// <param name="repository">The repository that is to be marked as visited.</param>
        /// <returns>A search context.</returns>
        public CodeDocRepositorySearchContext CloneWithSingleVisit(ICodeDocMemberRepository repository) {
            if(repository == null) throw new ArgumentNullException();
            Contract.Ensures(Contract.Result<CodeDocRepositorySearchContext>() != null);
            return CloneWithSingleVisit(repository, DetailLevel);
        }

        /// <summary>
        /// Clones this search context with only the given repository marked as visited and an overridden detail level.
        /// </summary>
        /// <param name="repository">The repository that is to be marked as visited.</param>
        /// <param name="detailLevel">The desired detail level.</param>
        /// <returns>A search context.</returns>
        public CodeDocRepositorySearchContext CloneWithSingleVisit(ICodeDocMemberRepository repository, CodeDocMemberDetailLevel detailLevel) {
            if(repository == null) throw new ArgumentNullException("repository");
            Contract.Ensures(Contract.Result<CodeDocRepositorySearchContext>() != null);
            var result = CloneWithoutVisits(detailLevel);
            result.Visit(repository);
            return result;
        }

        /// <summary>
        /// Performs a search using this search context for a code doc member model.
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The first member model that is found, null otherwise.</returns>
        public ICodeDocMember Search(CRefIdentifier cRef) {
            ICodeDocMemberRepository repository;
            while((repository = PopUnvisitedRepository()) != null){
                var model = repository.GetMemberModel(cRef, this, DetailLevel);
                if (model != null)
                    return model;
            }
            return null;
        }

        /// <summary>
        /// Clones this search context with only the given repository marked as unvisited.
        /// </summary>
        /// <param name="targetRepository">The repository that is to be unvisited.</param>
        /// <returns>A search context.</returns>
        public CodeDocRepositorySearchContext CloneWithOneUnvisited(ICodeDocMemberRepository targetRepository) {
            Contract.Ensures(Contract.Result<CodeDocRepositorySearchContext>() != null);
            return CloneWithOneUnvisited(targetRepository, DetailLevel);
        }

        /// <summary>
        /// Clones this search context with only the given repository marked as unvisited and an overridden detail level.
        /// </summary>
        /// <param name="targetRepository">The repository that is to be unvisited.</param>
        /// <param name="detailLevel">The desired detail level.</param>
        /// <returns>A search context.</returns>
        public CodeDocRepositorySearchContext CloneWithOneUnvisited(ICodeDocMemberRepository targetRepository, CodeDocMemberDetailLevel detailLevel) {
            Contract.Ensures(Contract.Result<CodeDocRepositorySearchContext>() != null);
            var result = CloneWithoutVisits(detailLevel);
            foreach (var repository in result.AllRepositories)
                if (repository != targetRepository)
                    result.Visit(repository);
            return result;
        }
    }
}
