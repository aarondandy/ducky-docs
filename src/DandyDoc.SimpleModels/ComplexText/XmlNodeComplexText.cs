using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class XmlNodeComplexText : ComplexTextList
	{

		public XmlNodeComplexText(XmlNode node, IList<IComplexTextNode> children) : base(children){
			if(node == null) throw new ArgumentNullException("node");
			Contract.Requires(children != null);
			Node = node;
		}

		public XmlNode Node { get; private set; }

	}
}
