using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace DandyDoc.ExternalVisibility
{

    /// <summary>
    /// Determines general external visibility for reflected members.
    /// </summary>
    /// <seealso cref="DandyDoc.ExternalVisibility.ExternalVisibilityKind"/>
    public static class ReflectionExternalVisibility
    {

        /// <summary>
        /// Determines external visibility for the given <paramref name="memberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">The member to test.</param>
        /// <returns>Calculated external visibility.</returns>
        public static ExternalVisibilityKind GetExternalVisibility(this MemberInfo memberInfo) {
            if (memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.EndContractBlock();
            if (memberInfo is Type)
                return GetExternalVisibility((Type)memberInfo);
            if (memberInfo is MethodBase)
                return GetExternalVisibility((MethodBase)memberInfo);
            if (memberInfo is PropertyInfo)
                return GetExternalVisibility((PropertyInfo)memberInfo);
            if (memberInfo is FieldInfo)
                return GetExternalVisibility((FieldInfo)memberInfo);
            if (memberInfo is EventInfo)
                return GetExternalVisibility((EventInfo)memberInfo);
            throw new NotSupportedException();
        }

        private static ExternalVisibilityKind GetNestedExternalVisibility(this Type typeInfo) {
            if (typeInfo == null) throw new ArgumentNullException("typeInfo");
            Contract.EndContractBlock();
            if (typeInfo.IsNestedPublic)
                return ExternalVisibilityKind.Public;
            if (typeInfo.IsNestedFamily || typeInfo.IsNestedFamORAssem)
                return ExternalVisibilityKind.Protected;
            return ExternalVisibilityKind.Hidden;
        }

        /// <summary>
        /// Determines external visibility for the given type.
        /// </summary>
        /// <param name="typeInfo">The type to test.</param>
        /// <returns>Calculated external visibility.</returns>
        public static ExternalVisibilityKind GetExternalVisibility(this Type typeInfo) {
            if(typeInfo == null) throw new ArgumentNullException("typeInfo");
            Contract.EndContractBlock();
            if (typeInfo.IsNested) {
                Contract.Assume(typeInfo.DeclaringType != null);
                var parentVisibility = GetExternalVisibility(typeInfo.DeclaringType);
                if(parentVisibility == ExternalVisibilityKind.Hidden)
                    return ExternalVisibilityKind.Hidden;
                var thisVisibility = GetNestedExternalVisibility(typeInfo);
                return ExternalVisibilityOperations.LeastVisible(parentVisibility, thisVisibility);
            }
            return typeInfo.IsPublic
                ? ExternalVisibilityKind.Public
                : ExternalVisibilityKind.Hidden;
        }

        /// <summary>
        /// Determines external visibility for the given method.
        /// </summary>
        /// <param name="methodBase">The method to test.</param>
        /// <returns>Calculated external visibility.</returns>
        public static ExternalVisibilityKind GetExternalVisibility(this MethodBase methodBase) {
            if(methodBase == null) throw new ArgumentNullException("methodBase");
            Contract.EndContractBlock();
            var typeVisibility = GetExternalVisibility(methodBase.DeclaringType);
            var fieldVisibility =
                methodBase.IsPublic
                    ? ExternalVisibilityKind.Public
                : (methodBase.IsFamily || methodBase.IsFamilyOrAssembly)
                    ? ExternalVisibilityKind.Protected
                : ExternalVisibilityKind.Hidden;
            return ExternalVisibilityOperations.LeastVisible(typeVisibility, fieldVisibility);
        }

        /// <summary>
        /// Determines external visibility for the given property.
        /// </summary>
        /// <param name="propertyInfo">The property to test.</param>
        /// <returns>Calculated external visibility.</returns>
        /// <remarks>
        /// Because properties accessors can have differing visibility it is recommended to test them independently.
        /// </remarks>
        public static ExternalVisibilityKind GetExternalVisibility(this PropertyInfo propertyInfo) {
            if(propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            Contract.EndContractBlock();
            var getMethodInfo = propertyInfo.GetGetMethod(true);
            var setMethodInfo = propertyInfo.GetSetMethod(true);
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
        /// Determines external visibility for the given field.
        /// </summary>
        /// <param name="fieldInfo">The field to test.</param>
        /// <returns>Calculated external visibility.</returns>
        public static ExternalVisibilityKind GetExternalVisibility(this FieldInfo fieldInfo) {
            if(fieldInfo == null) throw new ArgumentNullException("fieldInfo");
            Contract.EndContractBlock();
            var typeVisibility = GetExternalVisibility(fieldInfo.DeclaringType);
            var fieldVisibility =
                fieldInfo.IsPublic
                    ? ExternalVisibilityKind.Public
                : (fieldInfo.IsFamily || fieldInfo.IsFamilyOrAssembly)
                    ? ExternalVisibilityKind.Protected
                : ExternalVisibilityKind.Hidden;
            return ExternalVisibilityOperations.LeastVisible(typeVisibility, fieldVisibility);
        }

        /// <summary>
        /// Determines external visibility for the given event.
        /// </summary>
        /// <param name="eventInfo">The event to test.</param>
        /// <returns>Calculated external visibility.</returns>
        public static ExternalVisibilityKind GetExternalVisibility(this EventInfo eventInfo) {
            if(eventInfo == null) throw new ArgumentNullException("eventInfo");
            Contract.EndContractBlock();
            var methodInfo = eventInfo.GetRaiseMethod(true) ?? eventInfo.GetAddMethod(true);
            return methodInfo == null
                ? ExternalVisibilityKind.Hidden
                : GetExternalVisibility(methodInfo);
        }

    }
}
