using System;
using System.Diagnostics.Contracts;

namespace DandyDoc.Core
{
	public class SeeAlsoReference
	{

		public SeeAlsoReference(IDocumentableEntity target, string text) {
			if(null == target) throw new ArgumentNullException("target");
			Contract.EndContractBlock();

			Target = target;
			Text = text;
		}

		public string Text { get; private set; }

		public IDocumentableEntity Target { get; private set; }

	}
}
