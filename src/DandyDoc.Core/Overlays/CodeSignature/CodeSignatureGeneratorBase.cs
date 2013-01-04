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

		public virtual CodeSignature GenerateSignature(IMemberDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			if (definition is TypeDefinition)
				return GenerateSignature((TypeDefinition)definition);
			if (definition is MethodDefinition)
				return GenerateSignature((MethodDefinition)definition);
			if (definition is PropertyDefinition)
				return GenerateSignature((PropertyDefinition)definition);
			if (definition is FieldDefinition)
				return GenerateSignature((FieldDefinition)definition);
			if (definition is EventDefinition)
				return GenerateSignature((EventDefinition)definition);
			return null;
		}

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
