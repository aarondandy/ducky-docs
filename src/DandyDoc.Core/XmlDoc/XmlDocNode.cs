﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocNode
	{

		protected static readonly IList<XmlDocNode> EmptyXmlDocNodeList = new XmlDocNode[0];

		public XmlDocNode(XmlNode node)
			: this(node, null)
		{
			Contract.Requires(node != null);
		}

		public XmlDocNode(XmlNode node, IEnumerable<XmlDocNode> children) {
			if(node == null) throw new ArgumentNullException("node");
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

		public XmlDocNode Parent { get; private set; }

		public XmlNode Node { get; private set; }

		public IList<XmlDocNode> Children { get; private set; }

		public bool HasChildren {
			get {
				Contract.Ensures(Contract.Result<bool>() == Children.Count > 0);
				return Children.Count > 0;
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Node != null);
			Contract.Invariant(Children != null);
			Contract.Invariant(Contract.ForAll(Children, child => child != null));
		}

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

		public XmlDocNode PriorSibling {
			get { return PriorSiblings.FirstOrDefault(); }
		}

		public XmlDocNode NextSibling {
			get { return NextSiblings.FirstOrDefault(); }
		}

	}
}
