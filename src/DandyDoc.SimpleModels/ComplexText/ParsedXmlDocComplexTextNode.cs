using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels.ComplexText
{
	public abstract class ParsedXmlDocComplexTextNode : IComplexTextNode
	{

		private static readonly HashSet<string> BasicUnpackNodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase){
			"summary",
			"remarks",
			"example"
		};

		public static IComplexTextNode Convert(ParsedXmlNodeBase node){
			if (node == null)
				return null;

			if(node is ParsedXmlTextNode)
				return new StandardComplexText(((ParsedXmlTextNode)node).HtmlDecoded);

			if (node is ParsedXmlElementBase){
				var parsedElement = (ParsedXmlElementBase) node;
				var xmlElement = parsedElement.Element;
				if (BasicUnpackNodes.Contains(xmlElement.Name)){
					return ConvertToSingleComplexNode(parsedElement.Children);
				}
				if (parsedElement is ParsedXmlTypeparamrefElement){
					var specialized = (ParsedXmlTypeparamrefElement) parsedElement;
					return new TypeParamrefComplexText(specialized.TypeparamName, ConvertAll(specialized.Children));
				}
			}

			throw new NotSupportedException();
		}

		private static IList<IComplexTextNode> ConvertAll(IList<ParsedXmlNodeBase> nodes){
			Contract.Requires(nodes != null);
			Contract.Ensures(Contract.Result<IList<IComplexTextNode>>() != null);
			Contract.Ensures(Contract.Result<IList<IComplexTextNode>>().Count == nodes.Count);
			var results = new List<IComplexTextNode>(nodes.Count);
			for (int i = 0; i < nodes.Count; i++)
				results.Add(Convert(nodes[i]));
			return results;
		}

		private static IComplexTextNode ConvertToSingleComplexNode(IList<ParsedXmlNodeBase> items) {
			Contract.Requires(items != null);
			Contract.Ensures(items.Count == 0 ? Contract.Result<IComplexTextNode>() == null : Contract.Result<IComplexTextNode>() != null);
			if (items.Count == 0)
				return null;
			if (items.Count == 1)
				return Convert(items[0]);
			return new ComplexTextList(ConvertAll(items));

		}

	}
}
