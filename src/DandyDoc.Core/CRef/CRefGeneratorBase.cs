namespace DandyDoc.CRef
{
    /// <summary>
    /// The base code reference generator class which can be inherited
    /// from to create a specific implementation.
    /// </summary>
    /// <seealso cref="DandyDoc.CRef.ReflectionCRefGenerator"/>
    /// <seealso cref="T:DandyDoc.CRef.CecilCRefGenerator"/>
    public abstract class CRefGeneratorBase : ICRefGenerator
    {
        /// <summary>
        /// Base constructor for a code reference generator.
        /// </summary>
        /// <param name="includeTypePrefix">A flag indicating if generated crefs contain a type prefix.</param>
        protected CRefGeneratorBase(bool includeTypePrefix) {
            IncludeTypePrefix = includeTypePrefix;
        }

        /// <summary>
        /// A flag indicating if this generator adds a code reference type prefix.
        /// </summary>
        /// <seealso cref="DandyDoc.CRef.CRefIdentifier.TargetType"/>
        public bool IncludeTypePrefix { get; protected set; }

        /// <summary>
        /// Generates a code reference (cref) for a supported <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity to generate a cref for.</param>
        /// <returns>A code reference (cref) for the given <paramref name="entity"/>.</returns>
        public abstract string GetCRef(object entity);

    }
}
