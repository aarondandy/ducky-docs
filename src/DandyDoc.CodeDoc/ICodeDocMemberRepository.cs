using System;
using System.Collections.Generic;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
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
        /// <returns>The member model if found.</returns>
        ICodeDocMember GetMemberModel(string cRef);

        /// <summary>
        /// Gets a member model for a code reference.
        /// </summary>
        /// <param name="cRef">The code reference.</param>
        /// <returns>The member model if found.</returns>
        ICodeDocMember GetMemberModel(CRefIdentifier cRef);

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
