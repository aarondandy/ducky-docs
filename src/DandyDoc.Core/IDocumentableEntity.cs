using System.Collections.Generic;
using System.Xml;

namespace DandyDoc.Core
{
	public interface IDocumentableEntity
	{
		string Name { get; }
		ParsedXmlDoc Summary { get; }
		IList<ParsedXmlDoc> Remarks { get; }
		IList<ParsedXmlDoc> Examples { get; }
		IList<SeeAlsoReference> SeeAlso { get; }
		XmlNode XmlDocNode { get; }
		IDocumentableEntity ResolveCref(string cref);

		string Cref { get; }

	}
}
