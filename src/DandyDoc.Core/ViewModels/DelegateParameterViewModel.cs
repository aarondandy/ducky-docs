using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class DelegateParameterViewModel : IParameterViewModel
	{

		internal DelegateParameterViewModel(ParameterDefinition definition, TypeViewModel parent, ParsedXmlElementBase xmlDoc) {
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

		IDefinitionViewModel IParameterViewModel.Parent { get { return Parent; } }

		public TypeViewModel Parent { get; private set; }

		public string RequiresQuickSummary{
			get{
				return null;
			}
		}

	}
}
