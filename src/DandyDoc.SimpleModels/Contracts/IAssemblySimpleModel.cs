using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IAssemblySimpleModel : ISimpleModel
	{
		string AssemblyFileName { get; }

		IList<ITypeSimpleModel> Types { get; }
	}
}
