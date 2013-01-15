using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public abstract class DefinitionMemberSimpleModelBase<TDefinition> : DefinitionSimpleModelBase<TDefinition>
		where TDefinition : MemberReference, IMemberDefinition
	{
		protected DefinitionMemberSimpleModelBase(TDefinition definition, ITypeSimpleModel declaringModel)
			: base(definition, declaringModel.ContainingAssembly)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
			DeclaringModel = declaringModel;
		}

		public ITypeSimpleModel DeclaringModel { get; private set; }

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
