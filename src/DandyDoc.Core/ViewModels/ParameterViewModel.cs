using System;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class ParameterViewModel
	{

		internal ParameterViewModel(ParameterDefinition definition, MethodViewModel parent, ParsedXmlElementBase xmlDoc) {
			if(null == definition) throw new ArgumentNullException("definition");
			if(null == parent) throw new ArgumentNullException("parent");
			Contract.EndContractBlock();
			Definition = definition;
			Parent = parent;
			XmlDoc = xmlDoc;
		}

		public string DisplayName { get { return Definition.Name; } }

		public ParsedXmlElementBase XmlDoc { get; private set; }

		public bool HasXmlDoc { get { return XmlDoc != null; } }

		public ParameterDefinition Definition { get; private set; }

		public MethodViewModel Parent { get; private set; }

		public string RequiresQuickSummary{
			get{
				var name = Definition.Name;
				Contract.Assume(!String.IsNullOrEmpty(name));
				if (Parent.RequiresParameterNotNullOrEmpty(name))
					return "not null and not empty";
				if (Parent.RequiresParameterNotNull(name))
					return "not null";
				return null;
			}
		}

	}
}
