using System;
using System.Diagnostics.Contracts;

namespace DandyDoc.Overlays.CodeSignature
{
	[Obsolete]
	public class CodeSignature
	{

		public CodeSignature(string language, string code) {
			if(String.IsNullOrEmpty(language)) throw new ArgumentNullException("language");
			if(String.IsNullOrEmpty(code)) throw new ArgumentNullException("code");
			Contract.EndContractBlock();
			Language = language;
			Code = code;
		}

		public string Language { get; private set; }

		public string Code { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(!String.IsNullOrEmpty(Language));
			Contract.Invariant(!String.IsNullOrEmpty(Code));
		}

	}
}
