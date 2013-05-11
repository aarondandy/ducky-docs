using System;
using System.Diagnostics.Contracts;
using DandyDoc.Cecil;
using Mono.Cecil;

namespace DandyDoc.ExternalVisibility
{
    public static class CecilExternalVisibility
    {

        public static ExternalVisibilityKind GetExternalVisibility(this MemberReference reference) {
            if(reference == null) throw new ArgumentNullException("reference");
            Contract.EndContractBlock();

            var definition = reference.ToDefinition();
            if (definition != null)
                return definition.GetExternalVisibility();

            var genericParameter = reference as GenericParameter;
            if (genericParameter != null) {
                return (genericParameter.DeclaringMethod ?? (MemberReference)(genericParameter.DeclaringType))
                    .GetExternalVisibility();
            }

            var declaringType = reference.DeclaringType;
            if (declaringType != null)
                return declaringType.GetExternalVisibility();

            throw new NotSupportedException("Could not get a member definition.");
        }

        public static ExternalVisibilityKind GetExternalVisibility(this IMemberDefinition definition) {
            if (definition == null) throw new ArgumentNullException("definition");
            Contract.EndContractBlock();
            if (definition is TypeDefinition)
                return GetExternalVisibility((TypeDefinition)definition);
            if (definition is MethodDefinition)
                return GetExternalVisibility((MethodDefinition)definition);
            if (definition is PropertyDefinition)
                return GetExternalVisibility((PropertyDefinition)definition);
            if (definition is FieldDefinition)
                return GetExternalVisibility((FieldDefinition)definition);
            if (definition is EventDefinition)
                return GetExternalVisibility((EventDefinition)definition);
            throw new NotSupportedException();
        }

        private static ExternalVisibilityKind GetNestedExternalVisibility(this TypeDefinition typeDefinition) {
            if (typeDefinition == null) throw new ArgumentNullException("typeDefinition");
            Contract.EndContractBlock();
            if (typeDefinition.IsNestedPublic)
                return ExternalVisibilityKind.Public;
            if (typeDefinition.IsNestedFamily || typeDefinition.IsNestedFamilyOrAssembly)
                return ExternalVisibilityKind.Protected;
            return ExternalVisibilityKind.Hidden;
        }

        public static ExternalVisibilityKind GetExternalVisibility(this TypeDefinition typeDefinition) {
            if (typeDefinition == null) throw new ArgumentNullException("typeDefinition");
            Contract.EndContractBlock();
            if (typeDefinition.IsNested) {
                var parentVisibility = GetExternalVisibility(typeDefinition.DeclaringType);
                if (parentVisibility == ExternalVisibilityKind.Hidden)
                    return ExternalVisibilityKind.Hidden;
                var thisVisibility = GetNestedExternalVisibility(typeDefinition);
                return ExternalVisibilityKindOperations.Min(parentVisibility, thisVisibility);
            }
            return typeDefinition.IsPublic
                ? ExternalVisibilityKind.Public
                : ExternalVisibilityKind.Hidden;
        }

        public static ExternalVisibilityKind GetExternalVisibility(this MethodDefinition methodDefinition) {
            if (methodDefinition == null) throw new ArgumentNullException("methodDefinition");
            Contract.EndContractBlock();
            var typeVisibility = GetExternalVisibility(methodDefinition.DeclaringType);
            var fieldVisibility =
                methodDefinition.IsPublic
                    ? ExternalVisibilityKind.Public
                : (methodDefinition.IsFamily || methodDefinition.IsFamilyOrAssembly)
                    ? ExternalVisibilityKind.Protected
                : ExternalVisibilityKind.Hidden;
            return ExternalVisibilityKindOperations.Min(typeVisibility, fieldVisibility);
        }

        public static ExternalVisibilityKind GetExternalVisibility(this PropertyDefinition propertyDefinition) {
            if (propertyDefinition == null) throw new ArgumentNullException("propertyDefinition");
            Contract.EndContractBlock();
            var getMethodInfo = propertyDefinition.GetMethod;
            var setMethodInfo = propertyDefinition.SetMethod;
            if (getMethodInfo == null) {
                return setMethodInfo == null
                    ? ExternalVisibilityKind.Hidden
                    : GetExternalVisibility(setMethodInfo);
            }
            return setMethodInfo == null
                ? GetExternalVisibility(getMethodInfo)
                : ExternalVisibilityKindOperations.Max(
                    GetExternalVisibility(getMethodInfo),
                    GetExternalVisibility(setMethodInfo)
                );
        }

        public static ExternalVisibilityKind GetExternalVisibility(this FieldDefinition fieldDefinition) {
            if (fieldDefinition == null) throw new ArgumentNullException("fieldDefinition");
            Contract.EndContractBlock();
            var typeVisibility = GetExternalVisibility(fieldDefinition.DeclaringType);
            var fieldVisibility =
                fieldDefinition.IsPublic
                    ? ExternalVisibilityKind.Public
                : (fieldDefinition.IsFamily || fieldDefinition.IsFamilyOrAssembly)
                    ? ExternalVisibilityKind.Protected
                : ExternalVisibilityKind.Hidden;
            return ExternalVisibilityKindOperations.Min(typeVisibility, fieldVisibility);
        }

        public static ExternalVisibilityKind GetExternalVisibility(this EventDefinition eventDefinition) {
            if (eventDefinition == null) throw new ArgumentNullException("eventDefinition");
            Contract.EndContractBlock();
            var methodInfo = eventDefinition.InvokeMethod ?? eventDefinition.AddMethod;
            return methodInfo == null
                ? ExternalVisibilityKind.Hidden
                : GetExternalVisibility(methodInfo);
        }

    }
}
