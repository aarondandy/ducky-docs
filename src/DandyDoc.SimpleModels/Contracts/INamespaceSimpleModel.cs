using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface INamespaceSimpleModel : ISimpleModel
	{

		IList<ITypeSimpleModel> Types { get; }

		IList<IAssemblySimpleModel> Assemblies { get; }

	}
}
