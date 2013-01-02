using System;
using System.Linq;
using DandyDoc.Overlays.XmlDoc;
using System.Diagnostics.Contracts;

namespace DandyDoc.ViewModels
{
	public class RequiresViewModel
	{

		public RequiresViewModel(MethodViewModel parent, ParsedXmlContractCondition xmlDoc){
			if(null == parent) throw new ArgumentNullException("parent");
			if(null == xmlDoc) throw new ArgumentNullException("xmlDoc");
			Contract.EndContractBlock();
			Parent = parent;
			Xml = xmlDoc;
		}

		public MethodViewModel Parent { get; private set; }

		public ParsedXmlContractCondition Xml { get; private set; }

		public string RequiresParameterNotNullOrEmpty() {
			return Parent.Definition.Parameters
				.Select(p => p.Name)
				.FirstOrDefault(n => Parent.RequiresParameterNotNullOrEmpty(n));
		}

		public string RequiresParameterNotNull(){
			return Parent.Definition.Parameters
				.Select(p => p.Name)
				.FirstOrDefault(n => Parent.RequiresParameterNotNull(n));
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Parent);
			Contract.Invariant(null != Xml);
		}

	}
}
