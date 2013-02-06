using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.Utility;

namespace DandyDoc.Reflection
{
	public static class ReflectionUtilities
	{

		public static string GetFilePath(Assembly assembly) {
			if (assembly == null) throw new ArgumentNullException("assembly");
			Contract.EndContractBlock();

			var codeBase = assembly.CodeBase;
			Uri uri;
			if (Uri.TryCreate(codeBase, UriKind.Absolute, out uri) && "FILE".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
				return uri.AbsolutePath;

			return assembly.Location;
		}

		public static bool IsOperatorOverload(this MethodBase methodInfo) {
			if (methodInfo == null)
				return false;

			if (!methodInfo.IsStatic)
				return false;

			return CSharpOperatorNameSymbolMap.IsOperatorName(methodInfo.Name);
		}

		public static bool IsDelegateType(this Type type) {
			if (type == null)
				return false;

			var baseType = type.BaseType;
			if (baseType == null)
				return false;

			if (!"System.MulticastDelegate".Equals(baseType.FullName))
				return false;

			var methods = type.GetMethods();
			return methods.Length > 0
				&& methods.Any(x => "Invoke".Equals(x.Name));
		}

		public static ParameterInfo[] GetDelegateTypeParameters(this Type type) {
			Contract.Ensures(Contract.Result<ParameterInfo[]>() != null);

			if (!IsDelegateType(type))
				return new ParameterInfo[0];

			var invokeMethod = type.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
			if (invokeMethod == null)
				return new ParameterInfo[0];

			return invokeMethod.GetParameters();
		}

		public static object GetDelegateReturnType(this Type type) {
			if (!IsDelegateType(type))
				return null;

			var invokeMethod = type.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
			if (invokeMethod == null)
				return null;

			return invokeMethod.ReturnType;
		}

		public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type) {
			if (type == null) throw new ArgumentNullException("type");
			Contract.Ensures(Contract.Result<IEnumerable<ConstructorInfo>>() != null);

			return type.GetConstructors()
				.Concat(type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic))
				.Concat(type.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic));
		}

		public static IEnumerable<MethodInfo> GetAllMethods(this Type type) {
			if (type == null) throw new ArgumentNullException("type");
			Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);

			return type.GetMethods()
				.Concat(type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
				.Concat(type.GetMethods(BindingFlags.Static | BindingFlags.Public))
				.Concat(type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic));
		}

		public static IEnumerable<PropertyInfo> GetAllProperties(this Type type) {
			if (type == null) throw new ArgumentNullException("type");
			Contract.Ensures(Contract.Result<IEnumerable<PropertyInfo>>() != null);

			return type.GetProperties()
				.Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
				.Concat(type.GetProperties(BindingFlags.Static | BindingFlags.Public))
				.Concat(type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic));
		}

		public static IEnumerable<FieldInfo> GetAllFields(this Type type) {
			if (type == null) throw new ArgumentNullException("type");
			Contract.Ensures(Contract.Result<IEnumerable<FieldInfo>>() != null);

			return type.GetFields()
				.Concat(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
				.Concat(type.GetFields(BindingFlags.Static | BindingFlags.Public))
				.Concat(type.GetFields(BindingFlags.Static | BindingFlags.NonPublic));
		}

		public static IEnumerable<EventInfo> GetAllEvents(this Type type) {
			if (type == null) throw new ArgumentNullException("type");
			Contract.Ensures(Contract.Result<IEnumerable<EventInfo>>() != null);

			return type.GetEvents()
				.Concat(type.GetEvents(BindingFlags.Instance | BindingFlags.NonPublic))
				.Concat(type.GetEvents(BindingFlags.Static | BindingFlags.Public))
				.Concat(type.GetEvents(BindingFlags.Static | BindingFlags.NonPublic));
		}

		public static IList<Type> GetAllNestedTypes(this Type type) {
			if (type == null) throw new ArgumentNullException("type");
			Contract.Ensures(Contract.Result<IList<Type>>() != null);

			var result = new List<Type>(type.GetNestedTypes());
			result.AddRange(type.GetNestedTypes(BindingFlags.NonPublic));
			return result;
		} 

	}
}
