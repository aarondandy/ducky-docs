using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public abstract class ParsedXmlElementBase : ParsedXmlNodeBase
	{

		protected ParsedXmlElementBase(XmlElement element, CrefOverlay crefOverlay)
			: base(element, crefOverlay)
		{
			if(null == element) throw new ArgumentNullException("element");
			if(null == crefOverlay) throw new ArgumentNullException("crefOverlay");
			Contract.EndContractBlock();
		}

		public XmlElement Element { get { return (XmlElement)Node; } }

	}
}
