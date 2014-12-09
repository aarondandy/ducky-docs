using System.Collections.Generic;
using DuckyDocs.CRef;

namespace DuckyDocs.CodeDoc
{
    /// <summary>
    /// A repository that creates or retrieves code doc members.
    /// </summary>
    public interface ICodeDocMemberRepository
    {

        /// <summary>
        /// Gets a member model for a code reference.
        /// </summary>
        /// <param name="cRef">The code reference.</param>
        /// <param name="searchContext">The search context to use when locating other models.</param>
        /// <param name="detailLevel">Indicates the desired detail level of the generated model.</param>
        /// <returns>The member model if found.</returns>
        ICodeDocMember GetMemberModel(CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext = null, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full);

        /// <summary>
        /// The assemblies exposed by the repository.
        /// </summary>
        IList<CodeDocSimpleAssembly> Assemblies { get; }

        /// <summary>
        /// The namespaces exposed by the repository.
        /// </summary>
        IList<CodeDocSimpleNamespace> Namespaces { get; }

    }
}
