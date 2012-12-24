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

		public static ParsedXmlNodeBase Parse(XmlNode node, DefinitionXmlDocBase docBase){
			if(null == node) throw new ArgumentNullException("node");
			if (null == docBase) throw new ArgumentNullException("docBase");
			Contract.EndContractBlock();

			var element = node as XmlElement;
			if (null != element) {
				if ("SEE".Equals(element.Name, StringComparison.OrdinalIgnoreCase)){
					throw new NotImplementedException();
				}
				return new ParsedXmlElement(element, docBase);
			}
			else {
				throw new NotImplementedException();
			}
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

		public CrefOverlay CrefOverlay { get { return DocBase.CrefOverlay; } }

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
