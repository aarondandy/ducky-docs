using System;
using System.Diagnostics.Contracts;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class ParameterViewModel
	{

		public ParameterViewModel(ParameterDefinition definition, ParsedXmlElementBase xmlDoc) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			Definition = definition;
			XmlDoc = xmlDoc;
		}

		public string DisplayName { get { return Definition.Name; } }

		public ParsedXmlElementBase XmlDoc { get; private set; }

		public ParameterDefinition Definition { get; private set; }

	}
}
