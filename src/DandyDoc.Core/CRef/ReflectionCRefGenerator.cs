using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace DandyDoc.CRef
{
	public class ReflectionCRefGenerator : CRefGeneratorBase
	{

		public static readonly ReflectionCRefGenerator NoPrefix = new ReflectionCRefGenerator(false);

		protected static readonly ReflectionCRefGenerator NoPrefixForceGenericExpansion = new ReflectionCRefGenerator(false){
			ForceGenericExpansion = true
		};

		public ReflectionCRefGenerator()
			: this(true) { }

		public ReflectionCRefGenerator(bool includeTypePrefix)
			: base(includeTypePrefix)
		{
			ForceGenericExpansion = false;
		}

		protected bool ForceGenericExpansion { get; set; }

		public override string GetCRef(object entity) {
			Contract.Ensures(
				entity is MemberInfo
				? !String.IsNullOrEmpty(Contract.Result<string>())
				: null == Contract.Result<string>()
			);
			var memberInfo = entity as MemberInfo;
			if (memberInfo != null) {
				return GetCRef(memberInfo);
			}
			return null;
		}

		protected virtual string GetCRef(MemberInfo info){
			if(info == null) throw new ArgumentNullException("info");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

			if (info is Type)
				return GetCRef((Type) info);

			var type = info.DeclaringType;
			Contract.Assume(null != type);
			var typeCRef = NoPrefix.GetCRef(type);
			var memberCRef = info.Name;
			Contract.Assume(!String.IsNullOrEmpty(memberCRef));

			char crefTypePrefix = '\0';
			var methodInfo = info as MethodBase;
			if (null != methodInfo) {
				if (methodInfo is ConstructorInfo && memberCRef.Length > 1 && memberCRef[0] == '.') {
					memberCRef = '#' + memberCRef.Substring(1);
				}
				else if (methodInfo.IsGenericMethod) {
					memberCRef += String.Concat("``", methodInfo.GetGenericArguments().Length);
				}
				var methodParameters = methodInfo.GetParameters();
				if (methodParameters.Length > 0) {
					memberCRef += '(' + String.Join(",", methodParameters.Select(GetCRefParamTypeName)) + ')';
				}
				crefTypePrefix = 'M';
			}
			else if (info is PropertyInfo) {
				var propertyInfo = ((PropertyInfo)info);
				var propertyParameters = propertyInfo.GetIndexParameters();
				if (propertyParameters.Length > 0) {
					memberCRef += '(' + String.Join(",", propertyParameters.Select(GetCRefParamTypeName)) + ')';
				}
				crefTypePrefix = 'P';
			}
			else if (info is FieldInfo) {
				crefTypePrefix = 'F';
			}
			else if (info is EventInfo) {
				crefTypePrefix = 'E';
			}

			var cref = typeCRef + '.' + memberCRef;
			if (IncludeTypePrefix && crefTypePrefix != '\0')
				cref = String.Concat(crefTypePrefix, ':', cref);

			return cref;
		}

		private string GetCRefParamTypeName(ParameterInfo parameterInfo) {
			Contract.Requires(parameterInfo != null);
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			Contract.Assume(null != parameterInfo.ParameterType);
			return NoPrefixForceGenericExpansion.GetCRef(parameterInfo.ParameterType);
		}

		protected virtual string GetCRef(Type type) {
			if (type == null) throw new ArgumentNullException("type");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

			if (type.IsGenericParameter)
				return GetGenericParameterName(type);

			return GetFullName(type);
		}

		protected virtual string GetGenericParameterName(Type type) {
			if (type == null) throw new ArgumentNullException("type");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var declaringMethod = type.DeclaringMethod;
			if (null != declaringMethod){
				return String.Concat("``", type.GenericParameterPosition);
			}
			return String.Concat('`', type.GenericParameterPosition);
		}

		protected virtual string GetFullName(Type type){
			if(type == null) throw new ArgumentNullException("type");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var typeParts = new List<string>();
			var currentType = type;
			while (true) {
				var currentTypeName = currentType.Name;

				if (currentType.IsGenericType && (ForceGenericExpansion || !currentType.IsGenericTypeDefinition)){
					var tickIndex = currentTypeName.LastIndexOf('`');
					if (tickIndex >= 0)
						currentTypeName = currentTypeName.Substring(0, tickIndex);

					currentTypeName = String.Concat(
						currentTypeName,
						'{',
						String.Join(",", type.GetGenericArguments().Select(NoPrefixForceGenericExpansion.GetCRef)),
						'}'
					);
				}

				typeParts.Insert(0, currentTypeName);
				if (!currentType.IsNested)
					break;

				currentType = currentType.DeclaringType;
				Contract.Assume(null != currentType);
			}

			var ns = currentType.Namespace;
			var cref = typeParts.Count == 1
				? typeParts[0]
				: String.Join(".", typeParts);
			Contract.Assume(!String.IsNullOrEmpty(cref));

			if (!String.IsNullOrEmpty(ns))
				cref = ns + '.' + cref;

			if (IncludeTypePrefix)
				cref = "T:" + cref;

			return cref;
		}

	}
}
