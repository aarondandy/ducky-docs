using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.Cecil
{
    public static class CecilUtilities
    {

        private static readonly ReadOnlyCollection<ParameterDefinition> EmptyParameterDefinitionCollection = Array.AsReadOnly(new ParameterDefinition[0]);

        public static string GetFilePath(AssemblyDefinition assembly) {
            if (assembly == null) throw new ArgumentNullException("assembly");
            Contract.EndContractBlock();
            Contract.Assume(assembly.MainModule.FullyQualifiedName != null);
            return new FileInfo(assembly.MainModule.FullyQualifiedName).FullName;
        }

        public static bool IsOperatorOverload(this MethodDefinition methodDefinition) {
            if (methodDefinition == null)
                return false;

            if (!methodDefinition.IsStatic)
                return false;
            Contract.Assume(!String.IsNullOrEmpty(methodDefinition.Name));
            return CSharpOperatorNameSymbolMap.IsOperatorName(methodDefinition.Name);
        }

        public static bool IsDelegateType(this TypeDefinition typeDefinition) {
            if (typeDefinition == null)
                return false;

            var baseType = typeDefinition.BaseType;
            if (baseType == null)
                return false;

            if (!baseType.FullName.Equals("System.MulticastDelegate"))
                return false;

            return typeDefinition.HasMethods && typeDefinition.Methods.Any(x => "Invoke".Equals(x.Name));
        }

        public static IList<ParameterDefinition> GetDelegateTypeParameters(this TypeDefinition typeDefinition) {
            Contract.Ensures(Contract.Result<IList<ParameterDefinition>>() != null);
            if (!IsDelegateType(typeDefinition))
                return EmptyParameterDefinitionCollection;

            Contract.Assume(typeDefinition.Methods != null);
            var method = typeDefinition.Methods.FirstOrDefault(x => "Invoke".Equals(x.Name));
            return null == method || !method.HasParameters
                ? (IList<ParameterDefinition>)EmptyParameterDefinitionCollection
                : method.Parameters;
        }

    }
}
