using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DandyDoc.Core
{
	public class ParsedXmlDoc
	{

		public ParsedXmlDoc(string rawText, IDocumentableEntity boundEntity){
			RawText = rawText ?? String.Empty;
			BoundEntity = boundEntity;
		}

		public string RawText { get; private set; }
		public IDocumentableEntity BoundEntity { get; private set; }

		public IList<IParsedXmlPart> Parsed{
			get {
				throw new NotImplementedException();
			}
		} 

	}
}
