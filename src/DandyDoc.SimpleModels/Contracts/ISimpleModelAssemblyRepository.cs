using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ISimpleModelAssemblyRepository
	{

		ISimpleModelRepository RootRepository { get; }

		IAssemblySimpleModel Assembly { get; }

	}
}
