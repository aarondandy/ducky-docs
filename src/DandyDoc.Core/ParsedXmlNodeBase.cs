using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core
{
	public abstract class ParsedXmlNodeBase : IParsedXmlPart
	{
		protected ParsedXmlNodeBase(XmlNode node) {
			if(null == node) throw new ArgumentNullException("node");
			Contract.EndContractBlock();
			Node = node;
		}

		public XmlNode Node { get; private set; }

		public string RawXml { get { return Node.OuterXml; } }

		public string InnerXml { get { return Node.InnerXml; } }

		public string InnerText { get { return Node.InnerText; } }

	}
}
