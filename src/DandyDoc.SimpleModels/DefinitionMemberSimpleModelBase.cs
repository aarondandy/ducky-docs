using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public abstract class DefinitionMemberSimpleModelBase<TDefinition> : DefinitionSimpleModelBase<TDefinition>
		where TDefinition : MemberReference, IMemberDefinition
	{

		private static IAssemblySimpleModel GetContainingAssembly(ITypeSimpleModel declaringModel){
			Contract.Requires(null != declaringModel);
			Contract.Ensures(Contract.Result<IAssemblySimpleModel>() != null);
			Contract.Assume(declaringModel.ContainingAssembly != null);
			return declaringModel.ContainingAssembly;
		}

		protected DefinitionMemberSimpleModelBase(TDefinition definition, ITypeSimpleModel declaringModel)
			: base(definition, GetContainingAssembly(declaringModel))
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
			DeclaringTypeModel = declaringModel;
		}

		public override ISimpleModel DeclaringModel { get { return DeclaringTypeModel; } }

		public ITypeSimpleModel DeclaringTypeModel { get; private set; }

		public abstract override string SubTitle { get; }

		public override string NamespaceName {
			get { return DeclaringModel.NamespaceName; }
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(DeclaringModel != null);
		}

	}

}
