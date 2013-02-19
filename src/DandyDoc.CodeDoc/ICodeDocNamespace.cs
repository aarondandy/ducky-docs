using System.Collections.Generic;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocNamespace : ICodeDocEntity
	{

		IList<ICodeDocType> RootTypes { get; }

		IList<ICodeDocType> AllTypes { get; }

		IList<ICodeDocAssembly> Assemblies { get; }

	}
}
