using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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

				if (parsedElement is ParsedXmlListElement) {
					var specialized = (ParsedXmlListElement)parsedElement;
					return new DescriptionListComplexText(
						specialized.ListType,
						specialized.Items.Select(x =>
							new DescriptionListComplexText.ListItem(
								x.IsHeader,
								x.Term == null ? null : ConvertToSingleComplexNode(x.Term.Children),
								x.Description == null ? null : ConvertToSingleComplexNode(x.Description.Children))
						).ToList()
					);
				}

				var children = Convert(parsedElement.Children);

				if (BasicUnpackNodes.Contains(xmlElement.Name)){
					return ConvertToSingleComplexNode(children);
				}
				if (parsedElement is ParsedXmlTypeparamrefElement){
					var specialized = (ParsedXmlTypeparamrefElement) parsedElement;
					return new ParamrefComplexText(specialized.TypeparamName, children);
				}
				if (parsedElement is ParsedXmlParamrefElement){
					var specialied = (ParsedXmlParamrefElement) parsedElement;
					return new ParamrefComplexText(specialied.ParameterName, children);
				}
				if (parsedElement is ParsedXmlCode){
					var specialized = (ParsedXmlCode) parsedElement;
					return new CodeComplexText(specialized.Inline, specialized.Language, children);
				}
				if (parsedElement is ParsedXmlContractCondition){
					var specialized = (ParsedXmlContractCondition)parsedElement;
					return new ContractConditionComplexText(specialized, children);
				}
				if (parsedElement is ParsedCrefXmlElementBase){
					var specialized = (ParsedCrefXmlElementBase)parsedElement;
					string target;
					SeeComplexText.TargetKind kind;
					if (specialized is ParsedXmlSeeElement && !String.IsNullOrEmpty(((ParsedXmlSeeElement)specialized).HRef)){
						target = ((ParsedXmlSeeElement) specialized).HRef;
						kind = SeeComplexText.TargetKind.HRef;
					}
					else if (!String.IsNullOrEmpty(specialized.CRef)) {
						target = specialized.CRef;
						kind = SeeComplexText.TargetKind.CRef;
					}
					else if (!String.IsNullOrEmpty(specialized.HRef)) {
						target = specialized.HRef;
						kind = SeeComplexText.TargetKind.HRef;
					}
					else {
						target = String.Empty;
						kind = SeeComplexText.TargetKind.None;
					}
					return new SeeComplexText(target, kind, children);
				}
			}

			return new XmlNodeComplexText(node.Node, Convert(node.Children));
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

		public static IComplexTextNode ConvertToSingleComplexNode(IList<IComplexTextNode> items){
			if (items == null || items.Count == 0)
				return null;
			if (items.Count == 1)
				return items[0];
			return new ComplexTextList(items);
		}

		public static IComplexTextNode ConvertToSingleComplexNode(IList<ParsedXmlNodeBase> items) {
			if (items == null || items.Count == 0)
				return null;
			if (items.Count == 1)
				return Convert(items[0]);
			return new ComplexTextList(Convert(items));

		}

	}
}
