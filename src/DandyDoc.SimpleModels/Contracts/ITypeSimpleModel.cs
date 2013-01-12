using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ITypeSimpleModel : ISimpleModel
	{

		IList<ITypeSimpleModel> NestedTypes { get; }

		IList<IDelegateSimpleModel> NestedDelegates { get; }


	}
}
