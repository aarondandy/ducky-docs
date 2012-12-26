using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using DandyDoc.Core.Overlays.XmlDoc;

namespace DandyDoc.Core.ViewModels
{
	public class TypePageViewModel
	{

		private readonly Lazy<TypeDefinitionXmlDoc> _xmlDoc;

		public TypePageViewModel(TypeDefinition typeDefinition, XmlDocOverlay xmlDocOverlay){
			if(null == typeDefinition) throw new ArgumentNullException("typeDefinition");
			if(null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			Definition = typeDefinition;
			XmlDocOverlay = xmlDocOverlay;
			_xmlDoc = new Lazy<TypeDefinitionXmlDoc>(GetXmlDoc);
		}

		public TypeDefinition Definition { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public TypeDefinitionXmlDoc XmlDoc { get { return _xmlDoc.Value; } }

		private TypeDefinitionXmlDoc GetXmlDoc() {
			return XmlDocOverlay.GetDocumentation(Definition);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != Definition);
			Contract.Invariant(null != XmlDocOverlay);
		}

	}
}
