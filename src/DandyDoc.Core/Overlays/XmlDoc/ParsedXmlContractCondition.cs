using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Overlays.XmlDoc
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

		public bool IsEnsures {
			get { return Element.Name.StartsWith("ensures", StringComparison.OrdinalIgnoreCase); }
		}

		public bool IsEnsuresOnThrow {
			get { return "ensuresOnThrow".Equals(Element.Name, StringComparison.OrdinalIgnoreCase); }
		}

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

		public bool EnsuresResultNotNull{
			get{
				if (!IsEnsures)
					return false;

				var cSharp = CSharp;
				if (!String.IsNullOrEmpty(cSharp) && ("result != null".Equals(cSharp) || "null != result".Equals(cSharp)))
					return true;

				var vb = VisualBasic;
				if (!String.IsNullOrEmpty(vb) && ("result <> Nothing".Equals(vb) || "Nothing <> result".Equals(vb)))
					return true;

				return false;
			}
		}

		public bool EnsuresResultNotNullOrEmpty{
			get{
				if (!IsEnsures)
					return false;
				var cSharp = CSharp;
				if (!String.IsNullOrEmpty(cSharp) && "!IsNullOrEmpty(result)".Equals(cSharp))
					return true;

				var vb = VisualBasic;
				if (!String.IsNullOrEmpty(vb) && "Not IsNullOrEmpty(result)".Equals(vb))
					return true;

				return false;
			}
		}

		public bool RequiresParameterNotNull(string parameterName){
			if(String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			if (!IsRequires)
				return false;

			var cSharp = CSharp;
			if(!String.IsNullOrEmpty(cSharp)
				&& parameterName.Length + 8 == cSharp.Length
				&& (
					(cSharp.StartsWith(parameterName, StringComparison.Ordinal) && cSharp.EndsWith(" != null", StringComparison.Ordinal))
					|| (cSharp.EndsWith(parameterName, StringComparison.Ordinal) && cSharp.StartsWith("null != ", StringComparison.Ordinal))
				)
			){
				return true;
			}

			var vb = VisualBasic;
			if (!String.IsNullOrEmpty(vb)
				&& parameterName.Length + 11 == vb.Length
				&& (
					(vb.StartsWith(parameterName, StringComparison.Ordinal) && vb.EndsWith(" <> Nothing", StringComparison.Ordinal))
					|| (vb.EndsWith(parameterName, StringComparison.Ordinal) && vb.StartsWith("Nothing <> ", StringComparison.Ordinal))
				)
			) {
				return true;
			}

			return false;
		}

		public bool RequiresParameterNotNullOrEmpty(string parameterName) {
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			if (!IsRequires)
				return false;

			var cSharp = CSharp;
			if (!String.IsNullOrEmpty(cSharp)
				&& parameterName.Length + 16 == cSharp.Length
				&& cSharp.StartsWith("!IsNullOrEmpty(", StringComparison.Ordinal)
				&& cSharp[cSharp.Length - 1] == ')'
				&& cSharp.IndexOf(parameterName, StringComparison.Ordinal) == 15
				){
				return true;
			}

			var vb = VisualBasic;
			if (!String.IsNullOrEmpty(cSharp)
				&& parameterName.Length + 19 == vb.Length
				&& vb.StartsWith("Not IsNullOrEmpty(", StringComparison.Ordinal)
				&& vb[vb.Length - 1] == ')'
				&& vb.IndexOf(parameterName, StringComparison.Ordinal) == 15
			) {
				return true;
			}

			return false;
		}

	}
}
