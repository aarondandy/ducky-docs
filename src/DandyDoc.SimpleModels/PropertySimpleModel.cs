using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class PropertySimpleModel : DefinitionMemberSimpleModelBase<PropertyDefinition>, IPropertySimpleModel
	{

		public PropertySimpleModel(PropertyDefinition definition, ITypeSimpleModel declaringModel)
			: base(definition, declaringModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
		}

		public override string SubTitle {
			get {
				if (Definition.IsItemIndexerProperty())
					return "Indexer";
				return "Property";
			}
		}

	}
}
