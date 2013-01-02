using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Overlays.Cref;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.Overlays.XmlDoc
{
	public class MethodDefinitionXmlDoc : ParameterizedXmlDocBase
	{

		internal MethodDefinitionXmlDoc(MethodDefinition definition, XmlNode xmlNode, CrefOverlay crefOverlay)
			: base(definition, xmlNode, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != crefOverlay);
		}

		new public MethodDefinition Definition { get { return (MethodDefinition)(base.Definition); } }

		public IList<ParsedXmlContractCondition> Requires{
			get{
				var nodes = SelectParsedXmlNodes("requires");
				return null == nodes ? null : nodes.ConvertAll(n => (ParsedXmlContractCondition)n);
			}
		}

		public IList<ParsedXmlContractCondition> Ensures {
			get {
				var nodes = SelectParsedXmlNodes("ensures|ensuresOnThrow");
				return null == nodes ? null : nodes.ConvertAll(n => (ParsedXmlContractCondition)n);
			}
		}

	}
}
