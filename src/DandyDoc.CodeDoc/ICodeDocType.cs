using System.Collections.Generic;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocType : ICodeDocEntityContent, ICodeDocGenericDefinition
	{

		bool HasBaseChain { get; }

		IList<CRefIdentifier> BaseChainCRefs { get; }

		bool HasDirectInterfaces { get; }

        IList<CRefIdentifier> DirectInterfaceCRefs { get; }

	}
}
