using System.Collections.Generic;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocAssembly
	{

		string AssemblyFileName { get; }

        IList<CRefIdentifier> RootTypes { get; }

        IList<CRefIdentifier> AllTypes { get; }

	}
}
