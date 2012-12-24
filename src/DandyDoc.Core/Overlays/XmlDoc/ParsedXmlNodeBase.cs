using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Utility;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public abstract class ParsedXmlNodeBase
	{

		public static ParsedXmlNodeBase Parse(XmlNode node, CrefOverlay crefOverlay){
			if(null == node) throw new ArgumentNullException("node");
			if(null == crefOverlay) throw new ArgumentNullException("crefOverlay");
			Contract.EndContractBlock();

			var element = node as XmlElement;
			if (null != element) {
				if ("SEE".Equals(element.Name, StringComparison.OrdinalIgnoreCase)){
					throw new NotImplementedException();
				}
				return new ParsedXmlElement(element, crefOverlay);
			}
			else {
				throw new NotImplementedException();
			}
		}

		public ParsedXmlNodeBase(XmlNode node, CrefOverlay crefOverlay) {
			if(null == node) throw new ArgumentNullException("node");
			if(null == crefOverlay) throw new ArgumentNullException("crefOverlay");
			Contract.EndContractBlock();
			Node = node;
			CrefOverlay = crefOverlay;
		}

		public XmlNode Node { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public string NormalizedOuterXml{
			get{
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
				throw new NotImplementedException();
			}
		}

	}
}
