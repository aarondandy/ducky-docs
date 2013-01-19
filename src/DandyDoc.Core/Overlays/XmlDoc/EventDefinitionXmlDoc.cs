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

		internal EventDefinitionXmlDoc(EventDefinition definition, XmlNode xmlNode, CRefOverlay cRefOverlay)
			: base(definition, xmlNode, cRefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != cRefOverlay);
		}

		new public EventDefinition Definition { get { return (EventDefinition)(base.Definition); } }

	}
}
