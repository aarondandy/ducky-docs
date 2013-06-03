using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.Reflection;

namespace DandyDoc.CRef
{

    /// <summary>
    /// Code reference (cref) lookup for reflected members.
    /// </summary>
    public class ReflectionCRefLookup : CRefLookupBase<Assembly, MemberInfo>
    {

        /// <summary>
        /// Constructs a code reference lookup for the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The assemblies that are to be searched.</param>
        public ReflectionCRefLookup(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies) {
            Contract.Requires(assemblies != null);
        }

        /// <summary>
        /// Constructs a code reference lookup for the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The assemblies that are to be searched.</param>
        public ReflectionCRefLookup(IEnumerable<Assembly> assemblies)
            : base(assemblies) {
            if (assemblies == null) throw new ArgumentNullException("assemblies");
            Contract.EndContractBlock();
        }

        /// <summary>
        /// Locates a member based on a code reference.
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The member if found.</returns>
        public override MemberInfo GetMemberCore(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            return Assemblies
                .Select(x => GetMemberInfo(x, cRef))
                .FirstOrDefault(x => null != x);
        }

        private static MemberInfo GetMemberInfo(Assembly assembly, CRefIdentifier cRef) {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("T".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetType(assembly, cRef);
            if (cRef.HasTargetType)
                return GetNonTypeMemberInfo(assembly, cRef);

            return GetType(assembly, cRef)
                ?? GetNonTypeMemberInfo(assembly, cRef);
        }

        private static Type GetType(Assembly assembly, string typeName) {
            Contract.Requires(assembly != null);
            Contract.Requires(!String.IsNullOrEmpty(typeName));

            if (typeName.Contains(','))
                return null;

            foreach (var type in assembly.GetTypes()) {
                var typeNamespace = type.Namespace ?? String.Empty;
                if (typeName.Length <= typeNamespace.Length)
                    continue;

                string typeOnly;
                if (!String.IsNullOrEmpty(typeNamespace)) {
                    if (!typeName.StartsWith(typeNamespace))
                        continue;
                    Contract.Assume(typeName[typeNamespace.Length] == '.');
                    typeOnly = typeName.Substring(typeNamespace.Length + 1);
                    if (String.IsNullOrEmpty(typeOnly))
                        continue;
                }
                else {
                    typeOnly = typeName;
                }
                Contract.Assume(!String.IsNullOrEmpty(typeOnly));
                var result = ResolveTypeByName(type, typeOnly);
                if (null != result)
                    return result;
            }
            return null;
        }

        private static Type GetType(Assembly assembly, CRefIdentifier cRef) {
            Contract.Requires(assembly != null);
            Contract.Requires(cRef != null);
            var typeName = cRef.CoreName;
            if (String.IsNullOrEmpty(typeName))
                return null;
            return GetType(assembly, typeName);

        }

        private static Type ResolveTypeByName(Type type, string typeName) {
            Contract.Requires(type != null);
            Contract.Requires(!String.IsNullOrEmpty(typeName));
            var firstDotIndex = typeName.IndexOf('.');
            if (firstDotIndex < 0)
                return type.Name == typeName ? type : null;

            var nestedTypes = type.GetAllNestedTypes();
            if (nestedTypes.Length == 0)
                return null;
            var thisNamePart = typeName.Substring(0, firstDotIndex);
            if (type.Name != thisNamePart)
                return null;
            var offset = firstDotIndex + 1;
            if (offset >= typeName.Length)
                return null;

            var otherNamePart = typeName.Substring(offset);

            return nestedTypes
                .Select(nestedType => ResolveTypeByName(nestedType, otherNamePart))
                .FirstOrDefault(result => result != null);
        }

        private static MemberInfo GetNonTypeMemberInfo(Assembly assembly, CRefIdentifier cRef) {
            Contract.Requires(assembly != null);
            Contract.Requires(cRef != null);

            var lastDotIndex = cRef.CoreName.LastIndexOf('.');
            if (lastDotIndex <= 0 || (cRef.CoreName.Length - 1) == lastDotIndex)
                return null;

            var typeName = cRef.CoreName.Substring(0, lastDotIndex);
            var type = GetType(assembly, typeName);
            if (null == type)
                return null;

            var memberName = cRef.CoreName.Substring(lastDotIndex + 1);
            Contract.Assume(!String.IsNullOrEmpty(memberName));

            if (String.IsNullOrEmpty(cRef.TargetType) || "!".Equals(cRef.TargetType)) {
                var paramTypes = cRef.ParamPartTypes;
                return type.GetAllConstructors().FirstOrDefault(m => ConstructorMatches(m, memberName, paramTypes))
                    ?? type.GetAllMethods().FirstOrDefault(m => MethodMatches(m, memberName, paramTypes, cRef.ReturnTypePart))
                    ?? type.GetAllProperties().FirstOrDefault(p => PropertyMatches(p, memberName, paramTypes))
                    ?? type.GetAllEvents().FirstOrDefault(e => EventMatches(e, memberName))
                    ?? (type.GetAllFields().FirstOrDefault(f => FieldMatches(f, memberName)) as MemberInfo);
            }

            if ("M".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
                if (memberName.Length > 0 && memberName[0] == '#') // TODO: it would be better to handle the '#' as an escape of '.'
                    return type.GetAllConstructors().FirstOrDefault(m => ConstructorMatches(m, memberName, cRef.ParamPartTypes));
                return type.GetAllMethods().FirstOrDefault(m => MethodMatches(m, memberName, cRef.ParamPartTypes, cRef.ReturnTypePart));
            }

            if ("P".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return type.GetAllProperties().FirstOrDefault(p => PropertyMatches(p, memberName, cRef.ParamPartTypes));
            if ("F".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return type.GetAllFields().FirstOrDefault(f => FieldMatches(f, memberName));
            if ("E".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return type.GetAllEvents().FirstOrDefault(e => EventMatches(e, memberName));
            return null;
        }

        [Obsolete("It may be good to merge ConstructorMatches and MethodMatches.")]
        private static bool ConstructorMatches(ConstructorInfo methodInfo, string nameTest, IList<string> paramTypeTest) {
            Contract.Requires(methodInfo != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));

            if (nameTest.Length != methodInfo.Name.Length)
                return false;
            if (!nameTest.Equals(methodInfo.Name.Replace('.','#')))
                return false;

            var parameters = methodInfo.GetParameters();
            return parameters.Length > 0
                ? ParametersMatch(parameters, paramTypeTest)
                : null == paramTypeTest || paramTypeTest.Count == 0;
        }

        [Obsolete("It may be good to merge ConstructorMatches and MethodMatches.")]
        private static bool MethodMatches(MethodInfo methodInfo, string nameTest, IList<string> paramTypeTest, string returnTest) {
            Contract.Requires(methodInfo != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));

            if (!nameTest.Equals(methodInfo.Name)) {
                var genericArguments = methodInfo.GetGenericArguments();
                if (genericArguments.Length > 0 && nameTest.StartsWith(methodInfo.Name)) {
                    var methodGenericParamCount = genericArguments.Length;
                    if (!nameTest.Equals(methodInfo.Name + "``" + methodGenericParamCount)) {
                        return false;
                    }
                }
                else {
                    return false;
                }
            }

            var parameters = methodInfo.GetParameters();
            if (parameters.Length > 0 ? !ParametersMatch(parameters, paramTypeTest) : null != paramTypeTest && paramTypeTest.Count != 0)
                return false; // parameters to not match

            if(!String.IsNullOrEmpty(returnTest) && !ParametersMatch(methodInfo.ReturnType, returnTest))
                return false; // if a CRef return type is specified the return type must match

            return true;
        }

        private static bool PropertyMatches(PropertyInfo propertyInfo, string nameTest, IList<string> paramTypeTest) {
            Contract.Requires(propertyInfo != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));
            if (!nameTest.Equals(propertyInfo.Name))
                return false;

            var parameters = propertyInfo.GetIndexParameters();
            return parameters.Length > 0
                ? ParametersMatch(parameters, paramTypeTest)
                : null == paramTypeTest || paramTypeTest.Count == 0;
        }

        private static bool ParametersMatch(IList<ParameterInfo> parameters, IList<string> paramTypeTest) {
            Contract.Requires(parameters != null);
            if (null == paramTypeTest)
                return false;
            if (paramTypeTest.Count != parameters.Count)
                return false;
            for (int i = 0; i < paramTypeTest.Count; i++) {
                Contract.Assume(parameters[i].ParameterType != null);
                if (!ParametersMatch(parameters[i].ParameterType, paramTypeTest[i]))
                    return false;
            }
            return true;
        }

        private static bool ParametersMatch(Type parameterType, string paramTypeTest){
            Contract.Requires(parameterType != null);
            return String.Equals(paramTypeTest, ReflectionCRefGenerator.NoPrefixForceGenericExpansion.GetCRef(parameterType));
        }

        private static bool FieldMatches(FieldInfo fieldInfo, string nameTest) {
            Contract.Requires(fieldInfo != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));
            return nameTest.Equals(fieldInfo.Name);
        }

        private static bool EventMatches(EventInfo eventInfo, string nameTest) {
            Contract.Requires(eventInfo != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));
            return nameTest.Equals(eventInfo.Name);
        }

    }
}
