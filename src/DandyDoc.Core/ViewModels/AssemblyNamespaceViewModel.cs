using System;
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class AssemblyNamespaceViewModel
	{

		public AssemblyNamespaceViewModel(IMemberDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			SetFromReference((MemberReference)definition);
		}

		public AssemblyNamespaceViewModel(MemberReference reference) {
			if(null == reference) throw new ArgumentNullException("reference");
			Contract.EndContractBlock();
			SetFromReference(reference);
		}

		private void SetFromReference(MemberReference reference) {
			Contract.Requires(null != reference);
			
			var typeReference = reference as TypeReference;
			if (null != typeReference) {
				SetFromTypeReference(typeReference);
				return;
			}

			var memberDefinition = reference as IMemberDefinition;
			if (null != memberDefinition) {
				SetFromTypeReference(memberDefinition.DeclaringType);
				return;
			}

			throw new ArgumentOutOfRangeException("reference", "The given member reference is not supported.");
		}

		private TypeReference FindTopLevelType(TypeReference typeReference) {
			Contract.Requires(null != typeReference);
			Contract.Ensures(Contract.Result<TypeReference>() != null);
			while (typeReference.IsNested) {
				Contract.Assume(typeReference.DeclaringType != null);
				typeReference = typeReference.DeclaringType;
			}
			return typeReference;
		}

		private void SetFromTypeReference(TypeReference typeReference) {
			Contract.Requires(null != typeReference);
			typeReference = FindTopLevelType(typeReference);
			Namespace = typeReference.Namespace;
			Contract.Assume(null != typeReference.Module);
			Assembly = typeReference.Module.Assembly;
		}

		public string Namespace { get; private set; }
		public AssemblyDefinition Assembly { get; private set; }

	}
}
