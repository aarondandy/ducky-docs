using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc.Overlays.CodeSignature
{
	public class CodeSignatureOverlay
	{

		public CodeSignatureOverlay()
			: this(new[] {new CodeSignatureGeneratorCSharp()}) { }

		public CodeSignatureOverlay(IEnumerable<CodeSignatureGeneratorBase> generators) {
			if(null == generators) throw new ArgumentNullException("generators");
			Contract.EndContractBlock();
			Generators = Array.AsReadOnly(generators.ToArray());
		}

		public ReadOnlyCollection<CodeSignatureGeneratorBase> Generators { get; private set; } 

		public IList<CodeSignature> GenerateSignatures(IMemberDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(Contract.Result<IList<CodeSignature>>() != null);
			Contract.Ensures(Contract.ForAll(Contract.Result<IList<CodeSignature>>(), codeSignature => null != codeSignature), "Null code signatures will not be returned.");
			return Generators
				.Select(x => x.GenerateSignature(definition))
				.Where(x => null != x)
				.ToList();
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Generators != null);
		}

	}
}
