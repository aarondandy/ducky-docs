using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.SimpleModels.Contracts;
using DandyDoc.Overlays.XmlDoc;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class ContractConditionComplexText : ComplexTextList
	{


		public ContractConditionComplexText(ParsedXmlContractCondition core, IList<IComplexTextNode> children)
			: base(children)
		{
			if(null == core) throw new ArgumentNullException("core");
			Contract.Requires(children != null);
			Core = core;
		}

		protected ParsedXmlContractCondition Core { get; private set; }

		public bool IsRequires { get { return Core.IsRequires; } }

		public bool IsEnsures { get { return Core.IsEnsures; } }

		public bool IsInvariant { get { return Core.IsInvariant; } }

		public bool IsEnsuresOnThrow { get { return Core.IsEnsuresOnThrow; } }

		public string CodeForVisualBasic { get { return Core.VisualBasic; } }

		public string CodeForCSharp { get { return Core.CSharp; } }

	}
}
