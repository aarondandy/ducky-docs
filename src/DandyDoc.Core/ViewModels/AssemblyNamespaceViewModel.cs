using System;
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class AssemblyNamespaceViewModel
	{

		private static TypeReference ResolveTypeReference(MemberReference reference){
			Contract.Requires(null != reference);
			Contract.Ensures(Contract.Result<TypeReference>() != null);
			var typeReference = reference as TypeReference;
			if (null == typeReference){
				var memberDefinition = reference as IMemberDefinition;
				typeReference = null != memberDefinition
					? memberDefinition.DeclaringType
					: reference.DeclaringType;
			}
			Contract.Assume(null != typeReference);
			return typeReference;
		}

		private static TypeReference FindTopLevelType(TypeReference typeReference) {
			Contract.Requires(null != typeReference);
			Contract.Ensures(Contract.Result<TypeReference>() != null);
			while (typeReference.IsNested) {
				Contract.Assume(typeReference.DeclaringType != null);
				typeReference = typeReference.DeclaringType;
			}
			return typeReference;
		}

		public AssemblyNamespaceViewModel(IMemberDefinition definition)
			: this((MemberReference)definition)
		{
			Contract.Requires(null != definition);
		}

		public AssemblyNamespaceViewModel(MemberReference reference) {
			if(null == reference) throw new ArgumentNullException("reference");
			Contract.EndContractBlock();

			var typeReference = ResolveTypeReference(reference);
			typeReference = FindTopLevelType(typeReference);
			Namespace = typeReference.Namespace;
			Contract.Assume(null != typeReference.Module);
			Assembly = typeReference.Module.Assembly;
		}

		public string Namespace { get; private set; }

		public AssemblyDefinition Assembly { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Namespace != null);
			Contract.Invariant(Assembly != null);
		}

	}
}
