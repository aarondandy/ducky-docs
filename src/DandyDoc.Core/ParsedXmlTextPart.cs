using System;
using System.Diagnostics.Contracts;

namespace DandyDoc.Core
{
	public class ParsedXmlTextPart : IParsedXmlPart
	{

		public ParsedXmlTextPart(ParsedXmlDoc parent, int start, int length){
			if(null == parent) throw new ArgumentNullException("parent");
			Contract.EndContractBlock();
			StartIndex = start;
			Length = length;
		}

		public ParsedXmlDoc Parent { get; private set; }

		public int StartIndex { get; private set; }

		public int Length { get; private set; }

		public string RawText { get { return Parent.RawText.Substring(StartIndex, Length); } }

	}
}
