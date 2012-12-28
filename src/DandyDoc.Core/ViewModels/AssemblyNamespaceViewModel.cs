using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class AssemblyNamespaceViewModel
	{

		public AssemblyNamespaceViewModel(MemberReference reference) {
			if(null == reference) throw new ArgumentNullException("reference");
			Contract.EndContractBlock();

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

		private void SetFromTypeReference(TypeReference typeReference) {
			Contract.Requires(null != typeReference);
			Namespace = typeReference.Namespace;
			Contract.Assume(null != typeReference.Module);
			Assembly = typeReference.Module.Assembly;
		}

		public string Namespace { get; private set; }
		public AssemblyDefinition Assembly { get; private set; }

	}
}
