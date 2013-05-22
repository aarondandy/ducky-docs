using System;
using System.Diagnostics.Contracts;
using DandyDoc.Cecil;
using Mono.Cecil;

namespace DandyDoc.ExternalVisibility
{
    /// <summary>
    /// Determines general external visibility for Cecil members.
    /// </summary>
    /// <seealso cref="DandyDoc.ExternalVisibility.ExternalVisibilityKind"/>
    public static class CecilExternalVisibility
    {

        /// <summary>
        /// Determines external visibility for the given <paramref name="memberReference"/>.
        /// </summary>
        /// <param name="memberReference">The member to test.</param>
        /// <returns>Calculated external visibility.</returns>
        public static ExternalVisibilityKind GetExternalVisibilityOrDefault(this MemberReference memberReference, ExternalVisibilityKind defaultValue = ExternalVisibilityKind.Public) {
            if(memberReference == null) throw new ArgumentNullException("memberReference");
            Contract.EndContractBlock();

            var definition = memberReference.ToDefinition();
            if (definition != null)
                return definition.GetExternalVisibility();

            var genericParameter = memberReference as GenericParameter;
            if (genericParameter != null) {
                return (genericParameter.DeclaringMethod ?? (MemberReference)(genericParameter.DeclaringType))
                    .GetExternalVisibilityOrDefault(defaultValue);
            }

            var declaringType = memberReference.DeclaringType;
            if (declaringType != null)
                return declaringType.GetExternalVisibilityOrDefault(defaultValue);

            return defaultValue;
        }

        /// <summary>
        /// Determines external visibility for the given <paramref name="memberDefinition"/>.
        /// </summary>
        /// <param name="memberDefinition">The member to test.</param>
        /// <returns>Calculated external visibility.</returns>
        public static ExternalVisibilityKind GetExternalVisibility(this IMemberDefinition memberDefinition) {
            if (memberDefinition == null) throw new ArgumentNullException("memberDefinition");
            Contract.EndContractBlock();
            if (memberDefinition is TypeDefinition)
                return GetExternalVisibility((TypeDefinition)memberDefinition);
            if (memberDefinition is MethodDefinition)
                return GetExternalVisibility((MethodDefinition)memberDefinition);
            if (memberDefinition is PropertyDefinition)
                return GetExternalVisibility((PropertyDefinition)memberDefinition);
            if (memberDefinition is FieldDefinition)
                return GetExternalVisibility((FieldDefinition)memberDefinition);
            if (memberDefinition is EventDefinition)
                return GetExternalVisibility((EventDefinition)memberDefinition);
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

        /// <summary>
        /// Determines external visibility for the given <paramref name="typeDefinition"/>.
        /// </summary>
        /// <param name="typeDefinition">The type to test.</param>
        /// <returns>Calculated external visibility.</returns>
        public static ExternalVisibilityKind GetExternalVisibility(this TypeDefinition typeDefinition) {
            if (typeDefinition == null) throw new ArgumentNullException("typeDefinition");
            Contract.EndContractBlock();
            if (typeDefinition.IsNested) {
                var parentVisibility = GetExternalVisibility(typeDefinition.DeclaringType);
                if (parentVisibility == ExternalVisibilityKind.Hidden)
                    return ExternalVisibilityKind.Hidden;
                var thisVisibility = GetNestedExternalVisibility(typeDefinition);
                return ExternalVisibilityOperations.LeastVisible(parentVisibility, thisVisibility);
            }
            return typeDefinition.IsPublic
                ? ExternalVisibilityKind.Public
                : ExternalVisibilityKind.Hidden;
        }

        /// <summary>
        /// Determines external visibility for the given <paramref name="methodDefinition"/>.
        /// </summary>
        /// <param name="methodDefinition">The member to test.</param>
        /// <returns>Calculated external visibility.</returns>
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
            return ExternalVisibilityOperations.LeastVisible(typeVisibility, fieldVisibility);
        }

        /// <summary>
        /// Determines external visibility for the given <paramref name="propertyDefinition"/>.
        /// </summary>
        /// <param name="propertyDefinition">The member to test.</param>
        /// <returns>Calculated external visibility.</returns>
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
                : ExternalVisibilityOperations.MostVisible(
                    GetExternalVisibility(getMethodInfo),
                    GetExternalVisibility(setMethodInfo)
                );
        }

        /// <summary>
        /// Determines external visibility for the given <paramref name="fieldDefinition"/>.
        /// </summary>
        /// <param name="fieldDefinition">The member to test.</param>
        /// <returns>Calculated external visibility.</returns>
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
            return ExternalVisibilityOperations.LeastVisible(typeVisibility, fieldVisibility);
        }

        /// <summary>
        /// Determines external visibility for the given <paramref name="eventDefinition"/>.
        /// </summary>
        /// <param name="eventDefinition">The member to test.</param>
        /// <returns>Calculated external visibility.</returns>
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
