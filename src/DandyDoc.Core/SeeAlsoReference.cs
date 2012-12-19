using System;
using System.Diagnostics.Contracts;

namespace DandyDoc.Core
{
	public class SeeAlsoReference
	{

		public SeeAlsoReference(IDocumentableEntity target, ParsedXmlDoc description) {
			if(null == target) throw new ArgumentNullException("target");
			Contract.EndContractBlock();

			Target = target;
			Description = description;
		}

		public ParsedXmlDoc Description { get; private set; }

		public IDocumentableEntity Target { get; private set; }

	}
}
