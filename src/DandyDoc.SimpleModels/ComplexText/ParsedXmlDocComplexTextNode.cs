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
				var children = Convert(parsedElement.Children);
				if (BasicUnpackNodes.Contains(xmlElement.Name)){
					return ConvertToSingleComplexNode(children);
				}
				if (parsedElement is ParsedXmlTypeparamrefElement){
					var specialized = (ParsedXmlTypeparamrefElement) parsedElement;
					return new TypeParamrefComplexText(specialized.TypeparamName, children);
				}
				if (parsedElement is ParsedXmlCode){
					var specialized = (ParsedXmlCode) parsedElement;
					return new CodeComplexText(specialized.Inline, specialized.Language, children);
				}
				if (parsedElement is ParsedXmlContractCondition){
					var specialized = (ParsedXmlContractCondition)parsedElement;
					return new ContractConditionComplexText(specialized, children);
				}
			}

			throw new NotSupportedException();
		}

		public static IList<IComplexTextNode> Convert<T>(IList<T> nodes) where T : ParsedXmlNodeBase {
			if(null == nodes) throw new ArgumentNullException("nodes");
			Contract.Ensures(Contract.Result<IList<IComplexTextNode>>() != null);
			Contract.Ensures(Contract.Result<IList<IComplexTextNode>>().Count == nodes.Count);
			var results = new List<IComplexTextNode>(nodes.Count);
			for (int i = 0; i < nodes.Count; i++)
				results.Add(Convert(nodes[i]));
			return results;
		}

		private static IComplexTextNode ConvertToSingleComplexNode(IList<IComplexTextNode> items){
			Contract.Requires(items != null);
			Contract.Ensures(items.Count == 0 ? Contract.Result<IComplexTextNode>() == null : Contract.Result<IComplexTextNode>() != null);
			if (items.Count == 0)
				return null;
			if (items.Count == 1)
				return items[0];
			return new ComplexTextList(items);
		}

		private static IComplexTextNode ConvertToSingleComplexNode(IList<ParsedXmlNodeBase> items) {
			Contract.Requires(items != null);
			Contract.Ensures(items.Count == 0 ? Contract.Result<IComplexTextNode>() == null : Contract.Result<IComplexTextNode>() != null);
			if (items.Count == 0)
				return null;
			if (items.Count == 1)
				return Convert(items[0]);
			return new ComplexTextList(Convert(items));

		}

	}
}
