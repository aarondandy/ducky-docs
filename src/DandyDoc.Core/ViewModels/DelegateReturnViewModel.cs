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
	public class DelegateReturnViewModel : IReturnViewModel
	{

		internal DelegateReturnViewModel(TypeReference type, DelegateViewModel parent, ParsedXmlElementBase xmlDoc) {
			if(null == type) throw new ArgumentNullException("type");
			if(null == parent) throw new ArgumentNullException("parent");
			Contract.EndContractBlock();
			Type = type;
			Parent = parent;
			XmlDoc = xmlDoc;
		}

		public ParsedXmlElementBase XmlDoc { get; private set; }

		public bool HasXmlDoc { get { return XmlDoc != null; } }

		public TypeReference Type { get; private set; }

		IDefinitionViewModel IReturnViewModel.Parent { get { return Parent; } }

		public DelegateViewModel Parent { get; private set; }

		public string EnsuresQuickSummary{
			get { return null; }
		}

	}
}
