using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using DandyDoc.Reflection;
using DandyDoc.Utility;

namespace DandyDoc.DisplayName
{

	/// <summary>
	/// Generates display names for reflected members.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Note: the resulting display name can not be resolved back into the
	/// generating declaration or reference as it may be missing critical information.
	/// Use <see cref="DandyDoc.CRef.ReflectionCRefGenerator"/> if a unique and reversible
	/// identifying name is required.
	/// </para>
	/// </remarks>
	public class StandardReflectionDisplayNameGenerator
	{

		public StandardReflectionDisplayNameGenerator() {
			IncludeNamespaceForTypes = false;
			ShowGenericParametersOnDefinition = true;
			ShowTypeNameForMembers = false;
			ListSeperator = ", ";
		}

		public bool IncludeNamespaceForTypes { get; set; }

		public bool ShowGenericParametersOnDefinition { get; set; }

		public bool ShowTypeNameForMembers { get; set; }

		public string ListSeperator { get; set; }

		public string GetDisplayName(MemberInfo memberInfo) {
			if (null == memberInfo) throw new ArgumentNullException("memberInfo");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			if (memberInfo is Type)
				return GetDisplayName((Type)memberInfo, false);
			if (memberInfo is MethodBase)
				return GetDisplayName((MethodBase)memberInfo);
			if (memberInfo is PropertyInfo)
				return GetDisplayName((PropertyInfo)memberInfo);
			return GetGenericDisplayName(memberInfo);
		}

		public string GetDisplayName(MethodBase methodBase) {
			if (null == methodBase) throw new ArgumentNullException("methodBase");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			string name;
			if (methodBase.IsConstructor) {
				Contract.Assume(methodBase.DeclaringType != null);
				var typeName = methodBase.DeclaringType.Name;
				if (methodBase.DeclaringType.GetGenericArguments().Length > 0) {
					var tickIndex = typeName.LastIndexOf('`');
					if (tickIndex >= 0)
						typeName = typeName.Substring(0, tickIndex);
				}
				name = typeName;
			}
			else if (methodBase.IsOperatorOverload()) {
				if (CSharpOperatorNameSymbolMap.TryGetOperatorSymbol(methodBase.Name, out name)) {
					name = String.Concat("operator ", name);
				}
				else {
					name = methodBase.Name;
					if (name.StartsWith("op_"))
						name = name.Substring(3);
				}
			}
			else {
				name = methodBase.Name;
				var genericParameters = methodBase.GetGenericArguments();
				if (genericParameters.Length > 0) {
					var tickIndex = name.LastIndexOf('`');
					if (tickIndex >= 0)
						name = name.Substring(0, tickIndex);
					name = String.Concat(
						name,
						'<',
						String.Join(ListSeperator, genericParameters.Select(GetDisplayName)),
						'>');
				}
			}

			var parameters = methodBase.GetParameters();
			Contract.Assume(parameters != null);
			name = String.Concat(name, '(', GetParameterText(parameters), ')');

			if (ShowTypeNameForMembers) {
				Contract.Assume(null != methodBase.DeclaringType);
				name = String.Concat(GetDisplayName(methodBase.DeclaringType), '.', name);
			}

			return name;
		}

		public string GetDisplayName(PropertyInfo propertyInfo) {
			if (null == propertyInfo) throw new ArgumentNullException("propertyInfo");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var name = propertyInfo.Name;
			var parameters = propertyInfo.GetIndexParameters();
			Contract.Assume(parameters != null);
			if (parameters.Length > 0) {
				char openParen, closeParen;
				if ("Item".Equals(name)) {
					openParen = '[';
					closeParen = ']';
				}
				else {
					openParen = '(';
					closeParen = ')';
				}
				
				name = String.Concat(
					name,
					openParen,
					GetParameterText(parameters),
					closeParen);
			}
			if (ShowTypeNameForMembers) {
				Contract.Assume(null != propertyInfo.DeclaringType);
				name = String.Concat(GetDisplayName(propertyInfo.DeclaringType), '.', name);
			}
			return name;
		}

		public string GetDisplayName(Type type) {
			if (null == type) throw new ArgumentNullException("type");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return GetDisplayName(type, false);
		}

		protected virtual string GetParameterText(IEnumerable<ParameterInfo> parameters) {
			if (null == parameters) throw new ArgumentNullException("parameters");
			Contract.EndContractBlock();
			return String.Join(ListSeperator, parameters.Select(GetParameterText));
		}

		protected virtual string GetParameterText(ParameterInfo parameterInfo) {
			Contract.Requires(null != parameterInfo);
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			Contract.Assume(parameterInfo.ParameterType != null);
			return GetDisplayName(parameterInfo.ParameterType, false);
		}

		private string GetTypeDisplayName(Type type) {
			Contract.Requires(type != null);
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var result = type.Name;
			if (ShowGenericParametersOnDefinition) {
				var genericParameters = type.GetGenericArguments();
				if (genericParameters.Length > 0) {
					var tickIndex = result.LastIndexOf('`');
					if (tickIndex >= 0)
						result = result.Substring(0, tickIndex);

					if (type.IsNested) {
						Contract.Assume(type.DeclaringType != null);
						var parentGenericParameters = type.DeclaringType.GetGenericArguments();
						if(parentGenericParameters.Length > 0)
							genericParameters = genericParameters.Where(p => parentGenericParameters.All(t => t.Name != p.Name)).ToArray();
					}

					if (genericParameters.Length > 0) {
						result = String.Concat(
							result,
							'<',
							String.Join(
								ListSeperator,
								genericParameters.Select(GetDisplayName)),
							'>'
						);
					}
				}
			}
			return result;
		}

		public string GetDisplayName(Type type, bool hideParams) {
			if (null == type) throw new ArgumentNullException("type");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

			if (type.IsGenericParameter)
				return type.Name;

			var rootTypeReference = type;
			string fullTypeName;
			if (ShowTypeNameForMembers) {
				fullTypeName = GetNestedTypeDisplayName(ref rootTypeReference);
			}
			else {
				fullTypeName = GetTypeDisplayName(type);
				while (rootTypeReference.DeclaringType != null) {
					rootTypeReference = rootTypeReference.DeclaringType;
				}
			}

			if (IncludeNamespaceForTypes && !String.IsNullOrEmpty(rootTypeReference.Namespace))
				fullTypeName = String.Concat(rootTypeReference.Namespace, '.', fullTypeName);

			var definition = type as Type;
			if (null != definition) {
				if (definition.IsDelegateType() && !hideParams) {
					fullTypeName = String.Concat(fullTypeName, '(', GetParameterText(definition.GetDelegateTypeParameters()), ')');
				}
			}

			return fullTypeName;
		}

		private string GetNestedTypeDisplayName(ref Type type) {
			Contract.Requires(null != type);
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var typeParts = new List<string>();
			while (null != type) {
				typeParts.Insert(0, GetTypeDisplayName(type));
				if (!type.IsNested)
					break;

				Contract.Assume(null != type.DeclaringType);
				type = type.DeclaringType;
			}
			return typeParts.Count == 1
				? typeParts[0]
				: String.Join(".", typeParts);
		}

		private string GetGenericDisplayName(MemberInfo memberInfo) {
			Contract.Requires(null != memberInfo);
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var name = memberInfo.Name;
			if (ShowTypeNameForMembers) {
				Contract.Assume(null != memberInfo.DeclaringType);
				name = String.Concat(GetDisplayName(memberInfo.DeclaringType), '.', name);
			}
			return name;
		}

	}
}
