using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public interface IDefinitionXmlDoc
	{

		XmlNode Node { get; }

		ParsedXmlNodeBase Summary { get; }

		ParsedXmlNodeBase Remarks { get; }

	}
}
