using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using System.Collections.Generic;
using System;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class ParamrefComplexText : ComplexTextList
	{

		public ParamrefComplexText(string parameterName, IList<IComplexTextNode> children)
			: base(children)
		{
			Contract.Requires(children != null);
			ParameterName = parameterName ?? String.Empty;
		}

		public string ParameterName { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(ParameterName != null);
		}

	}

}
