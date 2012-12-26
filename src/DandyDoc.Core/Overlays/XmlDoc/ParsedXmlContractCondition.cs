using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class ParsedXmlContractCondition : ParsedXmlElementBase
	{

		internal ParsedXmlContractCondition(XmlElement element, DefinitionXmlDocBase docBase)
			: base(element, docBase)
		{
			Contract.Requires(element != null);
			Contract.Requires(docBase != null);
		}

		public bool IsRequires { get { return "requires".Equals(Element.Name, StringComparison.OrdinalIgnoreCase); } }

		public bool IsEnsures { get { return "ensures".Equals(Element.Name, StringComparison.OrdinalIgnoreCase); } }

		public bool IsInvariant { get { return "invariant".Equals(Element.Name, StringComparison.OrdinalIgnoreCase); } }

		public string CSharp{
			get {
				var attr = Element.Attributes["csharp"];
				return null == attr ? null : attr.Value;
			}
		}

		public string VisualBasic{
			get {
				var attr = Element.Attributes["vb"];
				return null == attr ? null : attr.Value;
			}
		}

		public string ExceptionCref{
			get{
				var attr = Element.Attributes["exception"];
				return null == attr ? null : attr.Value;
			}
		}

	}
}
