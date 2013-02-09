using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocContractElement : XmlDocRefElement
	{

		public XmlDocContractElement(XmlElement element, IEnumerable<XmlDocNode> children)
			: base(element, children)
		{
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
		}

		public bool IsRequires { get { return "REQUIRES".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

		public bool IsNormalEnsures { get { return "ENSURES".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

		public bool IsEnsuresOnThrow { get { return "ENSURESONTHROW".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

		public bool IsAnyEnsures { get { return IsNormalEnsures || IsEnsuresOnThrow; } }

		public bool IsInvariant { get { return "INVARIANT".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

		public string CSharp {
			get { return Element.GetAttribute("csharp"); }
		}

		public string VisualBasic {
			get { return Element.GetAttribute("vb"); }
		}

		public string ExceptionCRef {
			get { return Element.GetAttribute("exception"); }
		}

		public override string CRef {
			get {
				var result = base.CRef;
				if (String.IsNullOrEmpty(result))
					result = ExceptionCRef;

				return result;
			}
		}

	}
}
