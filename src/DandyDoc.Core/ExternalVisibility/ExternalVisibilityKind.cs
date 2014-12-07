namespace DuckyDocs.ExternalVisibility
{
    /// <summary>
    /// Specifies the level of external visibility for a member.
    /// </summary>
    public enum ExternalVisibilityKind
    {
        /// <summary>
        /// The member is hidden from external view.
        /// </summary>
        /// <remarks>
        /// If a member has a hidden visibility it may be family (internal) or private.
        /// </remarks>
        Hidden = 0,
        /// <summary>
        /// The member is externally visible only through inheritance.
        /// </summary>
        Protected = 1,
        /// <summary>
        /// The member is externally visible.
        /// </summary>
        Public = 3,
    }
}
