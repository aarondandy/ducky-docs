using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class MethodSimpleModel : DefinitionMemberSimpleModelBase<MethodDefinition>, IMethodSimpleModel
	{

		public MethodSimpleModel(MethodDefinition definition, ITypeSimpleModel declaringModel)
			: base(definition, declaringModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
		}

		public override string SubTitle {
			get {
				if (Definition.IsConstructor)
					return "Constructor";
				if (Definition.IsOperatorOverload())
					return "Operator";
				return "Method";
			}
		}


	}
}

