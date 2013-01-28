using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using DandyDoc.Utility;

namespace DandyDoc.XmlDoc
{
	public class XmlAssemblyDocumentation
	{

		private static XmlDocument Load(string filePath) {
			if (String.IsNullOrEmpty(filePath)) throw new ArgumentException("File path is not valid.", "filePath");
			Contract.Ensures(Contract.Result<XmlDocument>() != null);
			var doc = new XmlDocument();
			doc.Load(filePath);
			return doc;
		}

		public XmlAssemblyDocumentation(string filePath)
			: this(Load(filePath))
		{
			Contract.Requires(!String.IsNullOrEmpty(filePath));
			Parser = XmlDocParser.Default;
		}

		protected XmlAssemblyDocumentation(XmlDocument xmlDocument) {
			if (xmlDocument == null) throw new ArgumentNullException("xmlDocument");
			Contract.EndContractBlock();
			
			var members = xmlDocument.SelectNodes("/doc/members/member");
			if (null != members) {
				foreach (var member in members.Cast<XmlElement>()) {
					var subElements = member.ChildNodes.OfType<XmlElement>().ToList();
					foreach (var subElement in subElements) {
						var replacement = xmlDocument.CreateDocumentFragment();
						replacement.InnerXml = TextUtility.NormalizeAndUnindentElement(subElement.OuterXml) + "\n";
						Contract.Assume(null != subElement.ParentNode);
						subElement.ParentNode.ReplaceChild(replacement, subElement);
					}
				}
			}
			XmlDocument = xmlDocument;
		}

		protected XmlDocument XmlDocument { get; private set; }

		public XmlDocParser Parser { get; protected set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(XmlDocument != null);
		}

		public XmlElement GetMemberRawElement(string cRef) {
			if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef.", "cRef");
			Contract.EndContractBlock();

			return XmlDocument.SelectSingleNode(String.Format(
				"/doc/members/member[@name=\"{0}\"]", cRef)) as XmlElement;
		}

		public virtual XmlDocMember GetMember(string cRef) {
			if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef.", "cRef");
			Contract.EndContractBlock();
			var node = GetMemberRawElement(cRef);
			if (node == null)
				return null;
			return new XmlDocMember(node, node.ChildNodes.Cast<XmlNode>().Select(Parser.Parse));
		}

	}
}
