using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class FieldSimpleModel : DefinitionMemberSimpleModelBase<FieldDefinition>, IFieldSimpleModel
	{

		public FieldSimpleModel(FieldDefinition definition, ITypeSimpleModel declaringModel)
			: base(definition, declaringModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
		}

		public override string SubTitle {
			get {
				if (Definition.HasConstant)
					return "Constant";
				return "Field";
			}
		}

	}
}
