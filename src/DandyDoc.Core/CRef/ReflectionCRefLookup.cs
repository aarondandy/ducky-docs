using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.Reflection;

namespace DandyDoc.CRef
{

	public class ReflectionCRefLookup
	{

		private readonly ReadOnlyCollection<Assembly> _assemblies;

		public ReflectionCRefLookup(IEnumerable<Assembly> assemblies) {
			if (assemblies == null) throw new ArgumentNullException("assemblies");
			Contract.EndContractBlock();
			_assemblies = new ReadOnlyCollection<Assembly>(assemblies.ToArray());
		}

		public IList<Assembly> Assemblies {
			get {
				Contract.Ensures(Contract.Result<IList<Assembly>>() != null);
				return _assemblies;
			}
		}

		[Obsolete("More generic name.")]
		public virtual MemberInfo GetMemberInfo(string cRef) {
			if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
			Contract.EndContractBlock();
			return GetMemberInfo(new CRef(cRef));
		}

		[Obsolete("More generic name.")]
		public virtual MemberInfo GetMemberInfo(CRef cRef) {
			if (cRef == null) throw new ArgumentNullException("cRef");
			Contract.EndContractBlock();
			return _assemblies
				.Select(x => GetMemberInfo(x, cRef))
				.FirstOrDefault(x => null != x);
		}

		[Obsolete("More generic name.")]
		public static MemberInfo GetMemberInfo(Assembly assembly, CRef cRef) {
			if (assembly == null) throw new ArgumentNullException("assembly");
			if (cRef == null) throw new ArgumentNullException("cRef");
			Contract.EndContractBlock();

			if (cRef.IsTargetingType)
				return GetType(assembly, cRef);
			else if (cRef.HasTargetType)
				return GetNonTypeMemberInfo(assembly, cRef);

			return GetType(assembly, cRef)
				?? GetNonTypeMemberInfo(assembly, cRef);
		}

		private static Type GetType(Assembly assembly, string typeName) {
			Contract.Requires(assembly != null);
			Contract.Requires(!String.IsNullOrEmpty(typeName));
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

		private static Type GetType(Assembly assembly, CRef cRef) {
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
			if (nestedTypes.Count == 0)
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

		private static MemberInfo GetNonTypeMemberInfo(Assembly assembly, CRef cRef) {
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

			if (String.IsNullOrEmpty(cRef.TargetType)) {
				var paramTypes = cRef.ParamPartTypes;
				return type.GetAllConstructors().FirstOrDefault(m => ConstructorMatches(m, memberName, paramTypes))
					?? type.GetAllMethods().FirstOrDefault(m => MethodMatches(m, memberName, paramTypes))
					?? type.GetAllProperties().FirstOrDefault(p => PropertyMatches(p, memberName, paramTypes))
					?? type.GetAllEvents().FirstOrDefault(e => EventMatches(e, memberName))
					?? (type.GetAllFields().FirstOrDefault(f => FieldMatches(f, memberName)) as MemberInfo);
			}
			else if ("M".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
				if (memberName.Length > 0 && memberName[0] == '#') {
					return type.GetAllConstructors().FirstOrDefault(m => ConstructorMatches(m, memberName, cRef.ParamPartTypes));
				}
				return type.GetAllMethods().FirstOrDefault(m => MethodMatches(m, memberName, cRef.ParamPartTypes));
			}
			else if ("P".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.GetAllProperties().FirstOrDefault(p => PropertyMatches(p, memberName, cRef.ParamPartTypes));
			}
			else if ("F".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.GetAllFields().FirstOrDefault(f => FieldMatches(f, memberName));
			}
			else if ("E".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.GetAllEvents().FirstOrDefault(e => EventMatches(e, memberName));
			}
			else {
				return null;
			}
		}

		private static bool ConstructorMatches(ConstructorInfo methodInfo, string nameTest, IList<string> paramTypeTest) {
			Contract.Requires(methodInfo != null);
			Contract.Requires(!String.IsNullOrEmpty(nameTest));

			if (nameTest.Length != methodInfo.Name.Length)
				return false;
			if (!nameTest.Substring(1).Equals(methodInfo.Name.Substring(1)))
				return false;

			var parameters = methodInfo.GetParameters();
			return parameters.Length > 0
				? ParametersMatch(parameters, paramTypeTest)
				: null == paramTypeTest || paramTypeTest.Count == 0;
		}

		private static bool MethodMatches(MethodInfo methodInfo, string nameTest, IList<string> paramTypeTest) {
			Contract.Requires(methodInfo != null);
			Contract.Requires(!String.IsNullOrEmpty(nameTest));
			
			if (!nameTest.Equals(methodInfo.Name)) {
				var genericArguments = methodInfo.GetGenericArguments();
				if (genericArguments.Length > 0 && nameTest.StartsWith(methodInfo.Name)) {
					var methodGenericParamCount = genericArguments
						.Length;//.Count(x => x == methodDefinition);
					if (!nameTest.Equals(methodInfo.Name + "``" + methodGenericParamCount)) {
						return false;
					}
				}
				else {
					return false;
				}
			}

			var parameters = methodInfo.GetParameters();
			return parameters.Length > 0
				? ParametersMatch(parameters, paramTypeTest)
				: null == paramTypeTest || paramTypeTest.Count == 0;
		}

		private static bool PropertyMatches(PropertyInfo propertyInfo, string nameTest, IList<String> paramTypeTest) {
			Contract.Requires(propertyInfo != null);
			Contract.Requires(!String.IsNullOrEmpty(nameTest));
			if (!nameTest.Equals(propertyInfo.Name))
				return false;

			var parameters = propertyInfo.GetIndexParameters();
			return parameters.Length > 0
				? ParametersMatch(parameters, paramTypeTest)
				: null == paramTypeTest || paramTypeTest.Count == 0;
		}

		private static bool ParametersMatch(IList<ParameterInfo> parameters, IList<String> paramTypeTest) {
			Contract.Requires(parameters != null);
			if (null == paramTypeTest)
				return false;
			if (paramTypeTest.Count != parameters.Count)
				return false;
			for (int i = 0; i < paramTypeTest.Count; i++) {
				Contract.Assume(null != parameters[i].ParameterType);
				if (!String.Equals(paramTypeTest[i], ReflectionCRefGenerator.NoPrefixForceGenericExpansion.GetCRef(parameters[i].ParameterType)))
					return false;
			}
			return true;
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
