using System.Collections.Generic;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocType : ICodeDocEntity, ICodeDocGenericDefinition
	{

		bool HasBaseChain { get; }

		IList<string> BaseChainCRefs { get; }

		bool HasDirectInterfaces { get; }

		IList<string> DirectInterfaceCRefs { get; }

	}
}
