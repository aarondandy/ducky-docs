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



		public CodeSignatureOverlay(IEnumerable<CodeSignatureGeneratorBase> generators) {
			if(null == generators) throw new ArgumentNullException("generators");
			Contract.EndContractBlock();
			Generators = Array.AsReadOnly(generators.ToArray());
		}

		public ReadOnlyCollection<CodeSignatureGeneratorBase> Generators { get; private set; } 

		public CodeSignature GenerateSignature(string language, MethodDefinition definition) {
			if(String.IsNullOrEmpty(language)) throw new ArgumentException("Invalid language.", "language");
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return Generators
				.Where(g => g.Language == language)
				.Select(g => g.GenerateSignature(definition))
				.FirstOrDefault(s => s != null);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Generators != null);
		}

	}
}
