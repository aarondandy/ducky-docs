using System;
using System.Diagnostics.Contracts;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class ReturnViewModel
	{

		internal ReturnViewModel(TypeReference type, MethodViewModel parent, ParsedXmlElementBase xmlDoc) {
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

		public MethodViewModel Parent { get; private set; }

		public string EnsuresQuickSummary{
			get {
				if (Parent.EnsuresResultNotNullOrEmpty)
					return "not null and not empty";
				if (Parent.EnsuresResultNotNull)
					return "not null";
				return null;
			}
		}

	}
}
