namespace DandyDoc.CRef
{
    /// <summary>
    /// Generates a code reference (cref) given a supported member.
    /// </summary>
    /// <seealso cref="DandyDoc.CRef.CRefIdentifier"/>
    public interface ICRefGenerator
    {

        /// <summary>
        /// Generates a code reference (cref) 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        string GetCRef(object entity);

        /// <summary>
        /// When true the generator will include a type prefix such as <c>M:</c>
        /// which signifies the type of the member that is referenced.
        /// </summary>
        bool IncludeTypePrefix { get; }

    }
}
