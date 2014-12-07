using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DuckyDocs.XmlDoc
{

    /// <summary>
    /// Generates XML doc nodes from raw XML member elements.
    /// </summary>
    public class XmlDocParser
    {
        /// <summary>
        /// The default parser instance.
        /// </summary>
        public static readonly XmlDocParser Default = new XmlDocParser();

        private readonly Dictionary<string, Func<XmlElement, XmlDocElement>> _elementCreators;

        /// <summary>
        /// Creates a default XML doc parser.
        /// </summary>
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

        /// <summary>
        /// Generates an XML doc node from a raw XML node.
        /// </summary>
        /// <param name="node">The node to process.</param>
        /// <returns>An XML doc node.</returns>
        public virtual XmlDocNode Parse(XmlNode node) {
            if (node == null) throw new ArgumentNullException("node");
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

            if (node is XmlCharacterData)
                return new XmlDocTextNode((XmlCharacterData)node);

            return new XmlDocNode(node, GetChildren(node));
        }

        /// <summary>
        /// Processes a collection of raw XML nodes.
        /// </summary>
        /// <param name="nodes">The raw XML nodes to process.</param>
        /// <returns>The resulting XML doc nodes.</returns>
        protected virtual IEnumerable<XmlDocNode> Parse(IEnumerable<XmlNode> nodes) {
            if (nodes == null) throw new ArgumentNullException("nodes");
            Contract.Ensures(Contract.Result<IEnumerable<XmlDocNode>>() != null);
            return nodes.Select(Parse).Where(x => x != null);
        }

        /// <summary>
        /// Processes the child nodes of a raw XML node.
        /// </summary>
        /// <param name="node">A node containing children to be processed.</param>
        /// <returns>The resulting XML doc child nodes.</returns>
        protected virtual IEnumerable<XmlDocNode> GetChildren(XmlNode node) {
            if (node == null) throw new ArgumentNullException("node");
            Contract.Ensures(Contract.Result<IEnumerable<XmlDocNode>>() != null);
            return Parse(node.ChildNodes.Cast<XmlNode>());
        }

        /// <summary>
        /// Processes a raw XML element as an XML doc code element.
        /// </summary>
        /// <param name="element">The raw XML element to process.</param>
        /// <returns>An XML doc code element.</returns>
        protected virtual XmlDocCodeElement CreateCodeElement(XmlElement element) {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<XmlDocCodeElement>() != null);
            return new XmlDocCodeElement(element, GetChildren(element));
        }

        /// <summary>
        /// Processes a raw XML element as an XML doc reference element.
        /// </summary>
        /// <param name="element">The raw XML element to process.</param>
        /// <returns>An XMl doc reference element.</returns>
        protected virtual XmlDocRefElement CreateReferenceElement(XmlElement element) {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<XmlDocRefElement>() != null);
            return new XmlDocRefElement(element, GetChildren(element));
        }

        /// <summary>
        /// Processes a raw XML element as an XML doc name element.
        /// </summary>
        /// <param name="element">The raw XML element to process.</param>
        /// <returns>An XML doc name element.</returns>
        protected virtual XmlDocNameElement CreateNamedElement(XmlElement element) {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<XmlDocNameElement>() != null);
            return new XmlDocNameElement(element, GetChildren(element));
        }

        /// <summary>
        /// Processes a raw XML element as an XML doc definition list element.
        /// </summary>
        /// <param name="element">The raw XML element to process.</param>
        /// <returns>An XML doc definition list element.</returns>
        protected virtual XmlDocDefinitionList CreateDefinitionListElement(XmlElement element) {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<XmlDocDefinitionList>() != null);
            return new XmlDocDefinitionList(element, GetChildren(element));
        }

        /// <summary>
        /// Processes a raw XML element as an XML doc definition list item element.
        /// </summary>
        /// <param name="element">The raw XML element to process.</param>
        /// <returns>An XML doc definition list item element.</returns>
        protected virtual XmlDocDefinitionListItem CreateDefinitionListItemElement(XmlElement element) {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<XmlDocDefinitionListItem>() != null);
            return new XmlDocDefinitionListItem(element, GetChildren(element));
        }

        /// <summary>
        /// Processes a raw XML element as an XML doc contract element.
        /// </summary>
        /// <param name="element">The raw XML element to process.</param>
        /// <returns>An XML doc contract element</returns>
        protected virtual XmlDocContractElement CreateContractElement(XmlElement element) {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<XmlDocContractElement>() != null);
            return new XmlDocContractElement(element, GetChildren(element));
        }

    }
}
