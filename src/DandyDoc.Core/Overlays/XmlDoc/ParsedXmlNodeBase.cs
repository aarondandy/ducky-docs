using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Utility;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public abstract class ParsedXmlNodeBase
	{

		public static ParsedXmlNodeBase Parse(XmlNode node, DefinitionXmlDocBase docBase){
			if (null == node) throw new ArgumentNullException("node");
			if (null == docBase) throw new ArgumentNullException("docBase");
			Contract.EndContractBlock();

			var element = node as XmlElement;
			if (null != element) {
				if ("SEE".Equals(element.Name, StringComparison.OrdinalIgnoreCase))
					throw new NotImplementedException();
				if ("C".Equals(element.Name, StringComparison.OrdinalIgnoreCase))
					return new ParsedXmlCode(element, true, docBase);
				if ("CODE".Equals(element.Name, StringComparison.OrdinalIgnoreCase))
					return new ParsedXmlCode(element, false, docBase);
				if ("EXCEPTION".Equals(element.Name, StringComparison.OrdinalIgnoreCase))
					return new ParsedXmlException(element, docBase);
				if ("LIST".Equals(element.Name, StringComparison.OrdinalIgnoreCase))
					return new ParsedXmlListElement(element, docBase);
				if(
					null != element.ParentNode
					&& "LIST".Equals(element.ParentNode.Name, StringComparison.OrdinalIgnoreCase)
					&& (
						"LISTHEADER".Equals(element.Name, StringComparison.OrdinalIgnoreCase)
						|| "ITEM".Equals(element.Name, StringComparison.OrdinalIgnoreCase)
					)
				)
					return new ParsedXmlListItemElement(element, docBase);


				return new ParsedXmlElement(element, docBase);
			}
			if (node is XmlText){
				return new ParsedXmlTextNode((XmlText)node, docBase);
			}
			throw new NotSupportedException();
		}

		public ParsedXmlNodeBase(XmlNode node, DefinitionXmlDocBase docBase) {
			if(null == node) throw new ArgumentNullException("node");
			if (null == docBase) throw new ArgumentNullException("docBase");
			Contract.EndContractBlock();
			Node = node;
			DocBase = docBase;
		}

		public DefinitionXmlDocBase DocBase { get; private set; }

		public XmlNode Node { get; private set; }

		public CrefOverlay CrefOverlay{
			get {
				Contract.Ensures(Contract.Result<CrefOverlay>() != null);
				return DocBase.CrefOverlay;
			}
		}

		public string NormalizedOuterXml{
			get {
				return TextUtility.ExtractIndentedNormalizedInnerText(Node.OuterXml);
			}
		}

		public string NormalizedInnerXml{
			get{
				return TextUtility.ExtractIndentedNormalizedInnerText(Node.InnerXml);
			}
		}

		public IList<ParsedXmlNodeBase> Children {
			get{
				Contract.Ensures(Contract.Result<IList<ParsedXmlNodeBase>>() != null);
				return Node.ChildNodes
					.Cast<XmlNode>()
					.Select<XmlNode, ParsedXmlNodeBase>(Parse)
					.ToList();
			}
		}

		private ParsedXmlNodeBase Parse(XmlNode node){
			Contract.Requires(null != node);
			return Parse(node, DocBase);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != DocBase);
			Contract.Invariant(null != Node);
		}

	}
}
