using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IMethodSimpleModel : ISimpleModel
	{

		bool HasGenericParameters { get; }

		IList<IGenericParameterSimpleModel> GenericParameters { get; }

		bool HasParameters { get; }

		IList<IParameterSimpleModel> Parameters { get; }

	}
}
