
using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IDelegateSimpleModel : ITypeSimpleModel
	{

		bool HasParameters { get; }

		IList<IParameterSimpleModel> Parameters { get; }

	}
}
