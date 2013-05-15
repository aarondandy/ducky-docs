using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.XmlDoc
{
    /// <summary>
    /// A general XML doc node wrapper.
    /// </summary>
    public class XmlDocNode
    {

        /// <summary>
        /// An empty XML doc node collection.
        /// </summary>
        protected static readonly IList<XmlDocNode> EmptyXmlDocNodeList = new XmlDocNode[0];

        /// <summary>
        /// Creates a new XML doc node.
        /// </summary>
        /// <param name="node">The raw XML node to wrap.</param>
        public XmlDocNode(XmlNode node)
            : this(node, null) {
            Contract.Requires(node != null);
        }

        /// <summary>
        /// Creates a new XML doc node.
        /// </summary>
        /// <param name="node">The raw XML node to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        public XmlDocNode(XmlNode node, IEnumerable<XmlDocNode> children) {
            if (node == null) throw new ArgumentNullException("node");
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
            Node = node;
            if (null == children) {
                Children = EmptyXmlDocNodeList;
            }
            else {
                var childrenArray = children.ToArray();
                foreach (var child in childrenArray) {
                    if (child == null)
                        throw new ArgumentException("All children must be non-null.", "children");
                    child.Parent = this;
                }

                Children = new ReadOnlyCollection<XmlDocNode>(childrenArray);
            }
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(Node != null);
            Contract.Invariant(Children != null);
            Contract.Invariant(Contract.ForAll(Children, child => child != null));
        }

        /// <summary>
        /// The parent XML doc node.
        /// </summary>
        public XmlDocNode Parent { get; private set; }

        /// <summary>
        /// The raw wrapped XML node.
        /// </summary>
        public XmlNode Node { get; private set; }

        /// <summary>
        /// The XML doc child nodes.
        /// </summary>
        public IList<XmlDocNode> Children { get; private set; }

        /// <summary>
        /// Determines if this node has any child nodes.
        /// </summary>
        public bool HasChildren {
            get {
                Contract.Ensures(Contract.Result<bool>() == Children.Count > 0);
                return Children.Count > 0;
            }
        }

        /// <summary>
        /// Traverses the prior sibling XML doc nodes.
        /// </summary>
        public IEnumerable<XmlDocNode> PriorSiblings {
            get {
                if (Parent == null || !Parent.HasChildren)
                    yield break;

                var siblings = Parent.Children;
                for (int i = siblings.IndexOf(this) - 1; i >= 0; i--) {
                    yield return siblings[i];
                }
            }
        }

        /// <summary>
        /// Traverses the next sibling XML doc nodes.
        /// </summary>
        public IEnumerable<XmlDocNode> NextSiblings {
            get {
                if (Parent == null || !Parent.HasChildren)
                    yield break;

                var siblings = Parent.Children;
                var i = siblings.IndexOf(this);
                if (i < 0)
                    yield break;

                for (i++; i < siblings.Count; i++) {
                    yield return siblings[i];
                }
            }
        }

        /// <summary>
        /// Gets the prior sibling XML doc node if one exists.
        /// </summary>
        public XmlDocNode PriorSibling {
            get {
                return PriorSiblings.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the next sibling XML doc node if one exists.
        /// </summary>
        public XmlDocNode NextSibling {
            get {
                return NextSiblings.FirstOrDefault();
            }
        }

    }
}
