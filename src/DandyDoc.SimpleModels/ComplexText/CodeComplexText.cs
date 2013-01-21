using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class CodeComplexText : ComplexTextList
	{

		public CodeComplexText(bool isInline, string languageName, string code)
			: this(isInline, languageName, new IComplexTextNode[]{new StandardComplexText(code)}) { }

		public CodeComplexText(bool isInline, string languageName, IList<IComplexTextNode> children)
			: base(children)
		{
			Contract.Requires(children != null);
			IsInline = isInline;
			Language = languageName ?? String.Empty;
		}

		public bool IsInline { get; private set; }

		public string Language { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Language != null);
		}

	}
}
