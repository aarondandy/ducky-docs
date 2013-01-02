using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Overlays.XmlDoc;

namespace DandyDoc.ViewModels
{
	public class EnsuresViewModel
	{

		public EnsuresViewModel(MethodViewModel parent, ParsedXmlContractCondition xmlDoc) {
			if (null == parent) throw new ArgumentNullException("parent");
			if (null == xmlDoc) throw new ArgumentNullException("xmlDoc");
			Contract.EndContractBlock();
			Parent = parent;
			Xml = xmlDoc;
		}

		public MethodViewModel Parent { get; private set; }

		public ParsedXmlContractCondition Xml { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Parent);
			Contract.Invariant(null != Xml);
		}

	}
}
