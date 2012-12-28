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
	public class FieldDefinitionXmlDoc : DefinitionXmlDocBase
	{

		internal FieldDefinitionXmlDoc(FieldDefinition definition, XmlNode xmlNode, CrefOverlay crefOverlay)
			: base(definition, xmlNode, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != crefOverlay);
		}

		new public FieldDefinition Definition { get { return (FieldDefinition)(base.Definition); } }

		public ParsedXmlElementBase ValueDoc { get { return SelectParsedXmlNode("value") as ParsedXmlElementBase; } }

	}
}
