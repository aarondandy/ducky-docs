using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class DelegateSimpleModel : TypeSimpleModel, IDelegateSimpleModel
	{

		public DelegateSimpleModel(TypeDefinition definition, IAssemblySimpleModel assemblyModel)
			: base(definition, assemblyModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(assemblyModel != null);
		}

	}
}
