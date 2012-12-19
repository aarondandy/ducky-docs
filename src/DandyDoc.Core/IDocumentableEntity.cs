using System.Collections.Generic;

namespace DandyDoc.Core
{
	public interface IDocumentableEntity
	{

		string Summary { get; }
		string Remarks { get; }
		IList<SeeAlsoReference> SeeAlso { get; }

	}
}
