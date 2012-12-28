using System;
using System.Diagnostics.Contracts;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class ReturnViewModel
	{

		public ReturnViewModel(TypeReference type, ParsedXmlElementBase xmlDoc) {
			if(null == type) throw new ArgumentNullException("type");
			Contract.EndContractBlock();
			Type = type;
			XmlDoc = xmlDoc;
		}

		public ParsedXmlElementBase XmlDoc { get; private set; }

		public TypeReference Type { get; private set; }

	}
}
