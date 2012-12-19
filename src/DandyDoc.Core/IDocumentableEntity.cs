using System.Collections.Generic;

namespace DandyDoc.Core
{
	public interface IDocumentableEntity
	{

		ParsedXmlDoc Summary { get; }
		ParsedXmlDoc Remarks { get; }
		IList<SeeAlsoReference> SeeAlso { get; }

	}
}
