using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Overlays.Cref;
using Mono.Cecil;

namespace DandyDoc.Overlays.XmlDoc
{

	[Obsolete]
	public class PropertyDefinitionXmlDoc : ParameterizedXmlDocBase
	{

		internal PropertyDefinitionXmlDoc(PropertyDefinition definition, XmlNode xmlNode, CRefOverlay cRefOverlay)
			: base(definition, xmlNode, cRefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != cRefOverlay);
		}

		public new PropertyDefinition Definition { get { return (PropertyDefinition)(base.Definition); } }

		public ParsedXmlElementBase ValueDoc { get { return SelectParsedXmlNode("value") as ParsedXmlElementBase; } }

		public MethodDefinitionXmlDoc SetterDocs {
			get {
				var node = Node.SelectSingleNode("setter");
				if (null == node)
					return null;
				var method = Definition.SetMethod;
				if (null == method)
					return null;
				return new MethodDefinitionXmlDoc(method, node, CRefOverlay);
			}
		}

		public MethodDefinitionXmlDoc GetterDocs {
			get {
				var node = Node.SelectSingleNode("getter");
				if (null == node)
					return null;
				var method = Definition.GetMethod;
				if (null == method)
					return null;
				return new MethodDefinitionXmlDoc(method, node, CRefOverlay);
			}
		}

	}
}
