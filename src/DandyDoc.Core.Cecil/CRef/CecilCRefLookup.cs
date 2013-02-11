using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc.CRef
{
	public class CecilCRefLookup : CRefLookupBase<AssemblyDefinition, MemberReference>
	{

		public CecilCRefLookup(IEnumerable<AssemblyDefinition> assemblies)
			: base(assemblies)
		{
			if(assemblies == null) throw new ArgumentNullException("assemblies");
			Contract.EndContractBlock();
		}

		public override MemberReference GetMember(string cRef) {
			if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
			Contract.EndContractBlock();
			return GetMember(new CRef(cRef));
		}

		public override MemberReference GetMember(CRef cRef) {
			if(cRef == null) throw new ArgumentNullException("cRef");
			Contract.EndContractBlock();
			return Assemblies
				.Select(x => GetMemberReference(x, cRef))
				.FirstOrDefault(x => null != x);
		}

		public static MemberReference GetMemberReference(AssemblyDefinition assembly, CRef cRef) {
			if(assembly == null) throw new ArgumentNullException("assembly");
			if(cRef == null) throw new ArgumentNullException("cRef");
			Contract.EndContractBlock();

			if (cRef.IsTargetingType)
				return GetTypeDefinition(assembly, cRef);
			else if (cRef.HasTargetType)
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

		private static TypeDefinition GetTypeDefinition(AssemblyDefinition assembly, CRef cRef) {
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

		private static MemberReference GetNonTypeMemberDefinition(AssemblyDefinition assembly, CRef cRef) {
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

			if (String.IsNullOrEmpty(cRef.TargetType)) {
				var paramTypes = cRef.ParamPartTypes;
				return type.Methods.FirstOrDefault(m => MethodMatches(m, memberName, paramTypes))
					?? type.Properties.FirstOrDefault(p => PropertyMatches(p, memberName, paramTypes))
					?? type.Events.FirstOrDefault(e => EventMatches(e, memberName))
					?? (type.Fields.FirstOrDefault(f => FieldMatches(f, memberName)) as MemberReference);
			}
			else if ("M".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.Methods.FirstOrDefault(m => MethodMatches(m, memberName, cRef.ParamPartTypes));
			}
			else if ("P".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.Properties.FirstOrDefault(p => PropertyMatches(p, memberName, cRef.ParamPartTypes));
			}
			else if ("F".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.Fields.FirstOrDefault(f => FieldMatches(f, memberName));
			}
			else if ("E".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.Events.FirstOrDefault(e => EventMatches(e, memberName));
			}
			else {
				return null;
			}
		}

		private static bool MethodMatches(MethodDefinition methodDefinition, string nameTest, IList<string> paramTypeTest) {
			Contract.Requires(methodDefinition != null);
			Contract.Requires(!String.IsNullOrEmpty(nameTest));
			if (nameTest[0] == '#' && methodDefinition.Name[0] == '.') {
				if (nameTest.Length != methodDefinition.Name.Length)
					return false;
				if (!nameTest.Substring(1).Equals(methodDefinition.Name.Substring(1)))
					return false;
			}
			else if (!nameTest.Equals(methodDefinition.Name)) {
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

			return methodDefinition.HasParameters
				? ParametersMatch(methodDefinition.Parameters, paramTypeTest)
				: null == paramTypeTest || paramTypeTest.Count == 0;
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
				Contract.Assume(null != parameters[i].ParameterType);
				if (!String.Equals(paramTypeTest[i], CecilCRefGenerator.NoPrefix.GetCRef(parameters[i].ParameterType)))
					return false;
			}
			return true;
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
