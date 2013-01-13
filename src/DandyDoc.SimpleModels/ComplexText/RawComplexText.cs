using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class RawComplexText : IComplexTextModel
	{

		public RawComplexText(string text){
			Text = text ?? String.Empty;
		}

		public string Text { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Text != null);
		}

	}
}
