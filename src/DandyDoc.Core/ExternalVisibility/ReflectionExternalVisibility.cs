using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace DandyDoc.ExternalVisibility
{
    public static class ReflectionExternalVisibility
    {

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

        public static ExternalVisibilityKind GetExternalVisibility(this Type typeInfo) {
            if(typeInfo == null) throw new ArgumentNullException("typeInfo");
            Contract.EndContractBlock();
            if (typeInfo.IsNested) {
                var parentVisibility = GetExternalVisibility(typeInfo.DeclaringType);
                if(parentVisibility == ExternalVisibilityKind.Hidden)
                    return ExternalVisibilityKind.Hidden;
                var thisVisibility = GetNestedExternalVisibility(typeInfo);
                return ExternalVisibilityKindOperations.Min(parentVisibility, thisVisibility);
            }
            return typeInfo.IsPublic
                ? ExternalVisibilityKind.Public
                : ExternalVisibilityKind.Hidden;
        }

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
            return ExternalVisibilityKindOperations.Min(typeVisibility, fieldVisibility);
        }

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
                : ExternalVisibilityKindOperations.Max(
                    GetExternalVisibility(getMethodInfo),
                    GetExternalVisibility(setMethodInfo)
                );
        }

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
            return ExternalVisibilityKindOperations.Min(typeVisibility, fieldVisibility);
        }

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
