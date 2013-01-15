using System.Collections.Generic;
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


		public bool HasParameters {
			get { return Parameters.Count > 0; }
		}

		public IList<IParameterSimpleModel> Parameters {
			get{
				// TODO: extract the parameters from the invoke method maybe?
				return new IParameterSimpleModel[0];
			}
		}
	}
}
