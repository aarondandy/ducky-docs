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

		public virtual string RequiresParameterNotNullOrEmpty() {
			if (!Parent.Definition.HasParameters)
				return null;
			Contract.Assume(Parent.Definition.Parameters != null);
			return Parent.Definition.Parameters
				.Select(p => p.Name)
				.FirstOrDefault(n => Xml.RequiresParameterNotNullOrEmpty(n));
		}

		public virtual string RequiresParameterNotNull() {
			if (!Parent.Definition.HasParameters)
				return null;
			Contract.Assume(Parent.Definition.Parameters != null);
			return Parent.Definition.Parameters
				.Select(p => p.Name)
				.FirstOrDefault(n => Xml.RequiresParameterNotNull(n));
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Parent);
			Contract.Invariant(null != Xml);
		}

	}
}
