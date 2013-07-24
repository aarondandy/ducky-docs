using System;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A desired detail level used for code documentation member model generation.
    /// </summary>
    [Flags]
    public enum CodeDocMemberDetailLevel : byte
    {

        /// <summary>
        /// All members of <see cref="ICodeDocMember"/> excluding <see cref="ICodeDocMember.SummaryContents"/>.
        /// </summary>
        Minimum = 0,
        /// <summary>
        /// Includes <see cref="ICodeDocMember.SummaryContents"/>.
        /// </summary>
        Summary = 1,
        /// <summary>
        /// Includes members from <see cref="CodeDocMemberContentBase"/>.
        /// </summary>
        AdditionalContents = 2,
        /// <summary>
        /// Includes contained members for <see cref="CodeDocType"/> or <see cref="CodeDocProperty"/>.
        /// </summary>
        Members = 4,
        /// <summary>
        /// Includes inheritance information for a member.
        /// </summary>
        Inheritance = 8,
        /// <summary>
        /// Includes all available information.
        /// </summary>
        Full = Minimum | Summary | AdditionalContents | Members | Inheritance,
        /// <summary>
        /// A "medium" level that includes <see cref="Minimum"/>, <see cref="Summary"/>, and <see cref="Inheritance"/>.
        /// </summary>
        QuickSummary = Minimum | Summary | Inheritance

    }
}
