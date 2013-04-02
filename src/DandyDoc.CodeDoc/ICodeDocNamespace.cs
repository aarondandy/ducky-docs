using System.Collections.Generic;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocNamespace : ICodeDocEntityContent
	{

        IList<CRefIdentifier> RootTypes { get; }

        IList<ICodeDocAssembly> Assemblies { get; }

	}
}
