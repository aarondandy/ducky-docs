using System;
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.Overlays.CodeSignature
{
	public abstract class CodeSignatureGeneratorBase
	{

		protected CodeSignatureGeneratorBase(string language) {
			if(String.IsNullOrEmpty(language)) throw new ArgumentException("A valid ID is required.", "language");
			Contract.EndContractBlock();
			Language = language;
		}

		public string Language { get; private set; }

		public abstract CodeSignature GenerateSignature(TypeDefinition definition);

		public abstract CodeSignature GenerateSignature(MethodDefinition definition);

		public abstract CodeSignature GenerateSignature(PropertyDefinition definition);

		public abstract CodeSignature GenerateSignature(FieldDefinition definition);

		public abstract CodeSignature GenerateSignature(EventDefinition definition);

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(!String.IsNullOrEmpty(Language));
		}

	}
}
