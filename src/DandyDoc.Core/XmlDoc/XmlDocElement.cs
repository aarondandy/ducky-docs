using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocElement : XmlDocNode
	{

		public XmlDocElement(XmlElement element)
			: this(element, null)
		{
			Contract.Requires(element != null);
		}

		public XmlDocElement(XmlElement element, IEnumerable<XmlDocNode> children)
			: base(element, children)
		{
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
		}

		public XmlElement Element {
			get {
				Contract.Ensures(Contract.Result<XmlElement>() != null);
				return Node as XmlElement;
			}
		}

		public string Name {
			get {
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return Element.Name;
			}
		}

		public string OpenTagXml {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				var inner = Node.InnerXml;
				if (String.IsNullOrEmpty(inner))
					return Node.OuterXml;
				var outer = Node.OuterXml;
				var innerIndex = outer.IndexOf(inner, StringComparison.Ordinal);
				if (innerIndex <= 0)
					return String.Empty;
				return outer.Substring(0, innerIndex);
			}
		}

		public string CloseTagXml {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				var inner = Node.InnerXml;
				if (String.IsNullOrEmpty(inner))
					return String.Empty;
				var outer = Node.OuterXml;
				var innerIndex = outer.IndexOf(inner, StringComparison.Ordinal);
				if (innerIndex < 0)
					return String.Empty;
				return outer.Substring(innerIndex + inner.Length);
			}
		}

	}
}
