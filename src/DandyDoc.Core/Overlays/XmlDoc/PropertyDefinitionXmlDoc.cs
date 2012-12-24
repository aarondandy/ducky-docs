using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class PropertyDefinitionXmlDoc : ParameterizedXmlDocBase
	{

		internal PropertyDefinitionXmlDoc(PropertyDefinition definition, XmlNode xmlNode, CrefOverlay crefOverlay)
			: base(definition, xmlNode, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != crefOverlay);
		}

		public new PropertyDefinition Definition { get { return (PropertyDefinition)(base.Definition); } }

		public ParsedXmlNodeBase ValueDoc { get { return SelectParsedXmlNode("value"); } }

	}
}
