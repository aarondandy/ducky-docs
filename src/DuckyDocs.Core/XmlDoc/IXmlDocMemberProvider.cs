using System.Xml;

namespace DuckyDocs.XmlDoc
{

    /// <summary>
    /// Provides XML documentation members based on a code reference.
    /// </summary>
    public interface IXmlDocMemberProvider
    {

        /// <summary>
        /// Gets a raw XML documentation element for a given code reference (<paramref name="cRef"/>).
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The raw XML element if found.</returns>
        XmlElement GetMemberRawElement(string cRef);

        /// <summary>
        /// Gets a parsed XML documentation member element for a given code reference (<paramref name="cRef"/>).
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The parsed XML doc member element if found.</returns>
        XmlDocMember GetMember(string cRef);

    }
}
