using System.Collections.Generic;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocAssembly
	{

		string AssemblyFileName { get; }

		IList<ICodeDocType> RootTypes { get; }

		IList<ICodeDocType> AllTypes { get; }

	}
}
