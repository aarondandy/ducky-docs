namespace DuckyDocs.ExternalVisibility
{
    /// <summary>
    /// Utility methods for external visibility.
    /// </summary>
    public static class ExternalVisibilityOperations
    {

        /// <summary>
        /// Determines the least (min) visible value.
        /// </summary>
        /// <remarks>
        /// This method calculates the minimum value.
        /// The visibility of a member or type is often the least visibility between the member itself and its declaring type.
        /// When determining visibility along multiple levels this method is probably the right one to use.
        /// </remarks>
        /// <param name="a">A visibility.</param>
        /// <param name="b">A visibility.</param>
        /// <returns>The least visible value.</returns>
        public static ExternalVisibilityKind LeastVisible(ExternalVisibilityKind a, ExternalVisibilityKind b) {
            return a <= b ? a : b;
        }

        /// <summary>
        /// Determines the most (max) visible value.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// This method calculates the maximum value.
        /// <param name="a">A visibility.</param>
        /// <param name="b">A visibility.</param>
        /// <returns>The most visible value.</returns>
        public static ExternalVisibilityKind MostVisible(ExternalVisibilityKind a, ExternalVisibilityKind b) {
            return a >= b ? a : b;
        }

    }
}
