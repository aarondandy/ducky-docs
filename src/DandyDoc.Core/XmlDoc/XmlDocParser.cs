using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocParser
	{

		public static readonly XmlDocParser Default = new XmlDocParser();

		private readonly Dictionary<string, Func<XmlElement, XmlDocElement>>
			_elementCreators;

		public XmlDocParser() {
			_elementCreators = new Dictionary<string, Func<XmlElement, XmlDocElement>>(StringComparer.OrdinalIgnoreCase) {
				{"C", CreateCodeElement},
				{"CODE", CreateCodeElement},
				{"EXCEPTION", CreateReferenceElement},
				{"PERMISSION", CreateReferenceElement},
				{"SEE", CreateReferenceElement},
				{"SEEALSO", CreateReferenceElement},
				{"PARAM", CreateNamedElement},
				{"PARAMREF", CreateNamedElement},
				{"TYPEPARAM", CreateNamedElement},
				{"TYPEPARAMREF", CreateNamedElement},
				{"LIST", CreateDefinitionListElement},
				{"REQUIRES", CreateContractElement},
				{"ENSURES", CreateContractElement},
				{"ENSURESONTHROW", CreateContractElement},
				{"INVARIANT", CreateContractElement}
			};
		}

		public virtual XmlDocNode Parse(XmlNode node) {
			if(node == null) throw new ArgumentNullException("node");
			Contract.Ensures(Contract.Result<XmlDocNode>() != null);

			var element = node as XmlElement;
			if (null != element) {
				Func<XmlElement, XmlDocElement> creator;
				if (_elementCreators.TryGetValue(element.Name, out creator))
					return creator(element);

				if (
					XmlDocDefinitionListItem.IsItemElement(element)
					&& element.ParentNode != null
					&& "LIST".Equals(element.ParentNode.Name, StringComparison.OrdinalIgnoreCase)
				) {
					return CreateDefinitionListItemElement(element);
				}

				return new XmlDocElement(element, GetChildren(element));
			}

			if(node is XmlText)
				return new XmlDocTextNode((XmlText)node);

			return new XmlDocNode(node, GetChildren(node));
		}

		protected virtual IEnumerable<XmlDocNode> Parse(IEnumerable<XmlNode> nodes) {
			if(nodes == null) throw new ArgumentNullException("nodes");
			Contract.Ensures(Contract.Result<IEnumerable<XmlDocNode>>() != null);
			return nodes.Select(Parse).Where(x => x != null);
		}

		protected virtual IEnumerable<XmlDocNode> GetChildren(XmlNode node) {
			if(node == null) throw new ArgumentNullException("node");
			Contract.Ensures(Contract.Result<IEnumerable<XmlDocNode>>() != null);
			return Parse(node.ChildNodes.Cast<XmlNode>());
		}

		protected virtual XmlDocCodeElement CreateCodeElement(XmlElement element) {
			Contract.Requires(element != null);
			Contract.Ensures(Contract.Result<XmlDocCodeElement>() != null);
			return new XmlDocCodeElement(element, GetChildren(element));
		}

		protected virtual XmlDocRefElement CreateReferenceElement(XmlElement element) {
			Contract.Requires(element != null);
			Contract.Ensures(Contract.Result<XmlDocRefElement>() != null);
			return new XmlDocRefElement(element, GetChildren(element));
		}

		protected virtual XmlDocNameElement CreateNamedElement(XmlElement element) {
			Contract.Requires(element != null);
			Contract.Ensures(Contract.Result<XmlDocNameElement>() != null);
			return new XmlDocNameElement(element, GetChildren(element));
		}

		protected virtual XmlDocDefinitionList CreateDefinitionListElement(XmlElement element) {
			Contract.Requires(element != null);
			Contract.Ensures(Contract.Result<XmlDocDefinitionList>() != null);
			return new XmlDocDefinitionList(element, GetChildren(element));
		}

		protected virtual XmlDocDefinitionListItem CreateDefinitionListItemElement(XmlElement element) {
			Contract.Requires(element != null);
			Contract.Ensures(Contract.Result<XmlDocDefinitionListItem>() != null);
			return new XmlDocDefinitionListItem(element, GetChildren(element));
		}

		protected virtual XmlDocContractElement CreateContractElement(XmlElement element) {
			Contract.Requires(element != null);
			Contract.Ensures(Contract.Result<XmlDocContractElement>() != null);
			return new XmlDocContractElement(element, GetChildren(element));
		}

	}
}
