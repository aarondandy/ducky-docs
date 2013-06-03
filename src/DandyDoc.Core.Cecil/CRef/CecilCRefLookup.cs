using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc.CRef
{

    /// <summary>
    /// Code reference (cref) lookup for Cecil members.
    /// </summary>
    public class CecilCRefLookup : CRefLookupBase<AssemblyDefinition, MemberReference>
    {

        /// <summary>
        /// Constructs a code reference lookup for the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The assemblies that are to be searched.</param>
        public CecilCRefLookup(params AssemblyDefinition[] assemblies)
            : this((IEnumerable<AssemblyDefinition>)assemblies) {
            Contract.Requires(assemblies != null);
        }

        /// <summary>
        /// Constructs a code reference lookup for the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The assemblies that are to be searched.</param>
        public CecilCRefLookup(IEnumerable<AssemblyDefinition> assemblies)
            : base(assemblies) {
            if (assemblies == null) throw new ArgumentNullException("assemblies");
            Contract.EndContractBlock();
        }

        /// <summary>
        /// Locates a member based on a code reference.
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The member if found.</returns>
        public override MemberReference GetMemberCore(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            return Assemblies
                .Select(x => GetMemberReference(x, cRef))
                .FirstOrDefault(x => null != x);
        }

        private static MemberReference GetMemberReference(AssemblyDefinition assembly, CRefIdentifier cRef) {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("T".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetTypeDefinition(assembly, cRef);
            if (cRef.HasTargetType)
                return GetNonTypeMemberDefinition(assembly, cRef);

            return GetTypeDefinition(assembly, cRef)
                ?? GetNonTypeMemberDefinition(assembly, cRef);
        }

        private static TypeDefinition GetTypeDefinition(AssemblyDefinition assembly, string typeName) {
            Contract.Requires(assembly != null);
            Contract.Requires(!String.IsNullOrEmpty(typeName));
            Contract.Assume(null != assembly.Modules);
            foreach (var type in assembly.Modules.SelectMany(x => x.Types)) {
                var typeNamespace = type.Namespace;
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

        private static TypeDefinition GetTypeDefinition(AssemblyDefinition assembly, CRefIdentifier cRef) {
            Contract.Requires(assembly != null);
            Contract.Requires(cRef != null);
            var typeName = cRef.CoreName;
            if (String.IsNullOrEmpty(typeName))
                return null;
            return GetTypeDefinition(assembly, typeName);

        }

        private static TypeDefinition ResolveTypeByName(TypeDefinition type, string typeName) {
            Contract.Requires(type != null);
            Contract.Requires(!String.IsNullOrEmpty(typeName));
            var firstDotIndex = typeName.IndexOf('.');
            if (firstDotIndex < 0)
                return type.Name == typeName ? type : null;

            if (!type.HasNestedTypes)
                return null;
            var thisNamePart = typeName.Substring(0, firstDotIndex);
            if (type.Name != thisNamePart)
                return null;
            var offset = firstDotIndex + 1;
            if (offset >= typeName.Length)
                return null;

            var otherNamePart = typeName.Substring(offset);

            return type.NestedTypes
                .Select(nestedType => ResolveTypeByName(nestedType, otherNamePart))
                .FirstOrDefault(result => result != null);
        }

        private static MemberReference GetNonTypeMemberDefinition(AssemblyDefinition assembly, CRefIdentifier cRef) {
            Contract.Requires(assembly != null);
            Contract.Requires(cRef != null);

            var lastDotIndex = cRef.CoreName.LastIndexOf('.');
            if (lastDotIndex <= 0 || (cRef.CoreName.Length - 1) == lastDotIndex)
                return null;

            var typeName = cRef.CoreName.Substring(0, lastDotIndex);
            var type = GetTypeDefinition(assembly, typeName);
            if (null == type)
                return null;

            var memberName = cRef.CoreName.Substring(lastDotIndex + 1);
            Contract.Assume(!String.IsNullOrEmpty(memberName));

            if (String.IsNullOrEmpty(cRef.TargetType) || "!".Equals(cRef.TargetType)) {
                var paramTypes = cRef.ParamPartTypes;
                return type.Methods.FirstOrDefault(m => MethodMatches(m, memberName, paramTypes, cRef.ReturnTypePart))
                    ?? type.Properties.FirstOrDefault(p => PropertyMatches(p, memberName, paramTypes))
                    ?? type.Events.FirstOrDefault(e => EventMatches(e, memberName))
                    ?? (type.Fields.FirstOrDefault(f => FieldMatches(f, memberName)) as MemberReference);
            }

            if ("M".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return type.Methods.FirstOrDefault(m => MethodMatches(m, memberName, cRef.ParamPartTypes, cRef.ReturnTypePart));
            if ("P".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return type.Properties.FirstOrDefault(p => PropertyMatches(p, memberName, cRef.ParamPartTypes));
            if ("F".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return type.Fields.FirstOrDefault(f => FieldMatches(f, memberName));
            if ("E".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return type.Events.FirstOrDefault(e => EventMatches(e, memberName));
            return null;
        }

        private static bool MethodMatches(MethodDefinition methodDefinition, string nameTest, IList<string> paramTypeTest, string returnTest) {
            Contract.Requires(methodDefinition != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));
            if (!nameTest.Equals(methodDefinition.Name.Replace('.', '#'))) {
                if (methodDefinition.HasGenericParameters && nameTest.StartsWith(methodDefinition.Name)) {
                    var methodGenericParamCount = methodDefinition.GenericParameters
                        .Count(x => x.Owner == methodDefinition);
                    if (!nameTest.Equals(methodDefinition.Name + "``" + methodGenericParamCount)) {
                        return false;
                    }
                }
                else {
                    return false;
                }
            }

            if (methodDefinition.HasParameters ? !ParametersMatch(methodDefinition.Parameters, paramTypeTest) : null != paramTypeTest && paramTypeTest.Count != 0)
                return false; // parameters to not match

            if (!String.IsNullOrEmpty(returnTest) && !( ParametersMatch(methodDefinition.ReturnType, returnTest)))
                return false; // if a CRef return type is specified the return type must match

            return true;
        }

        private static bool PropertyMatches(PropertyDefinition propertyDefinition, string nameTest, IList<String> paramTypeTest) {
            Contract.Requires(propertyDefinition != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));
            if (!nameTest.Equals(propertyDefinition.Name))
                return false;

            return propertyDefinition.HasParameters
                ? ParametersMatch(propertyDefinition.Parameters, paramTypeTest)
                : null == paramTypeTest || paramTypeTest.Count == 0;
        }

        private static bool ParametersMatch(IList<ParameterDefinition> parameters, IList<String> paramTypeTest) {
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

        private static bool ParametersMatch(TypeReference parameterType, string paramTypeTest) {
            Contract.Requires(parameterType != null);
            return String.Equals(paramTypeTest, CecilCRefGenerator.NoPrefix.GetCRef(parameterType));
        }

        private static bool FieldMatches(FieldDefinition fieldDefinition, string nameTest) {
            Contract.Requires(fieldDefinition != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));
            return nameTest.Equals(fieldDefinition.Name);
        }

        private static bool EventMatches(EventDefinition eventDefinition, string nameTest) {
            Contract.Requires(eventDefinition != null);
            Contract.Requires(!String.IsNullOrEmpty(nameTest));
            return nameTest.Equals(eventDefinition.Name);
        }

    }
}
