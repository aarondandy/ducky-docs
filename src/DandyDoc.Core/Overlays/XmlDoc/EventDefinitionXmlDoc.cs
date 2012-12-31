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
	public class EventDefinitionXmlDoc : DefinitionXmlDocBase
	{

		internal EventDefinitionXmlDoc(EventDefinition definition, XmlNode xmlNode, CrefOverlay crefOverlay)
			: base(definition, xmlNode, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != crefOverlay);
		}

		new public EventDefinition Definition { get { return (EventDefinition)(base.Definition); } }

	}
}
