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

	}
}
