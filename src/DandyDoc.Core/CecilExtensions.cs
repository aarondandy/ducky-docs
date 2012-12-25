using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;
using System.Collections.ObjectModel;

namespace DandyDoc.Core
{
	public static class CecilExtensions
	{

		public static bool IsDelegateType(this TypeDefinition typeDefinition){
			if (null == typeDefinition)
				return false;

			var baseType = typeDefinition.BaseType;
			if (null == baseType)
				return false;

			if (!baseType.FullName.Equals("System.MulticastDelegate"))
				return false;

			return typeDefinition.HasMethods && typeDefinition.Methods.Any(x => "Invoke".Equals(x.Name));
		}

		private static readonly ReadOnlyCollection<ParameterDefinition> EmptyParameterDefinitionCollection = Array.AsReadOnly(new ParameterDefinition[0]);

		public static IList<ParameterDefinition> GetDelegateTypeParameters(this TypeDefinition typeDefinition){
			Contract.Ensures(Contract.Result<IEnumerable<ParameterDefinition>>() != null);
			if (!IsDelegateType(typeDefinition))
				return EmptyParameterDefinitionCollection;

			var method = typeDefinition.Methods.FirstOrDefault(x => "Invoke".Equals(x.Name));
			return null == method || !method.HasParameters
				? (IList<ParameterDefinition>)EmptyParameterDefinitionCollection
				: method.Parameters;
		}

	}
}
