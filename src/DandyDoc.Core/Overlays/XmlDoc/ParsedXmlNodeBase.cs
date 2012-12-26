﻿using System;
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
			if (null != element){
				var cmp = StringComparer.OrdinalIgnoreCase;
				if (cmp.Equals("C", element.Name))
					return new ParsedXmlCode(element, true, docBase);
				if (cmp.Equals("CODE",element.Name))
					return new ParsedXmlCode(element, false, docBase);
				if (cmp.Equals("EXCEPTION",element.Name))
					return new ParsedXmlException(element, docBase);
				if (cmp.Equals("PERMISSION", element.Name))
					return new ParsedXmlPermission(element, docBase);
				if (cmp.Equals("LIST", element.Name))
					return new ParsedXmlListElement(element, docBase);
				if (cmp.Equals("PARA", element.Name))
					return new ParsedXmlParagraphElement(element, docBase);
				if (cmp.Equals("PARAMREF", element.Name))
					return new ParsedXmlParamrefElement(element, docBase);
				if (cmp.Equals("TYPEPARAMREF", element.Name))
					return new ParsedXmlTypeparamrefElement(element, docBase);
				if (cmp.Equals("SEE", element.Name) || cmp.Equals("SEEALSO", element.Name))
					return new ParsedXmlSeeElement(element, docBase);
				if (cmp.Equals("REQUIRES", element.Name) || cmp.Equals("ENSURES", element.Name) || cmp.Equals("INVARIANT", element.Name))
					return new ParsedXmlContractCondition(element, docBase);

				if (
					null != element.ParentNode
					&& cmp.Equals("LIST", element.ParentNode.Name)
					&& (cmp.Equals("LISTHEADER", element.Name) || cmp.Equals("ITEM", element.Name))
				){
					return new ParsedXmlListItemElement(element, docBase);
				}

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
