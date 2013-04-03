using System.Collections.Generic;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocNamespace : ICodeDocEntity
	{

        IList<ICodeDocEntity> Types { get; }

        IList<ICodeDocAssembly> Assemblies { get; }

	}
}
