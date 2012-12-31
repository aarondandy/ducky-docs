using System;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace DandyDoc.Utility
{
	internal static class NameUtility
	{

		public static string ParentDotSeperatedName(string name) {
			if (String.IsNullOrEmpty(name))
				return null;

			int dotIndex = name.LastIndexOf('.');
			if (dotIndex <= 0)
				return name;

			return name.Substring(0, dotIndex);
		}

		public static string GetLastNamePart(string name) {
			if (String.IsNullOrEmpty(name))
				return null;

			int dotIndex = name.LastIndexOf('.');
			if (dotIndex <= 0)
				return name;

			return name.Substring(dotIndex+1);
		}


		public static string[] SplitNamespaceParts(string namespaceName){
			return namespaceName.Split('.');
		}

		private static readonly Regex GenericExpandedTypeRegex = new Regex(@"^(.+)[{](.*)[}]$", RegexOptions.Compiled);

		public static bool TryConvertToStandardTypeName(ref string typeName){
			if (String.IsNullOrEmpty(typeName))
				return false;

			var match = GenericExpandedTypeRegex.Match(typeName);
			if (!match.Success)
				return false;

			var namePart = match.Groups[1].Value;
			var genericPart = match.Groups[2].Value;
			if (String.IsNullOrWhiteSpace(genericPart))
				return false;

			var commaCount = genericPart.Count(x => x == ',');
			typeName = namePart + '`' + (commaCount + 1);
			return true;
		}

		private static readonly Regex GenericTypeNameRegex = new Regex(@"^(.+)[`](\d+)$", RegexOptions.Compiled);

		public static string TypeNameWithoutGenericParameterCountSuffix(string typeName){
			if (String.IsNullOrEmpty(typeName))
				return typeName;

			var match = GenericTypeNameRegex.Match(typeName);
			if (!match.Success)
				return typeName;

			return match.Groups[1].Value;
		}

		public static string GetCref(TypeReference type) {
			if (type.IsGenericParameter) {
				var genericType = type as GenericParameter;
				if (null != genericType) {
					var owner = genericType.Owner;
					var i = owner.GenericParameters.IndexOf(genericType);
					if (i >= 0) {
						if (owner is MethodDefinition) {
							return "``" + i;
						}
						if (owner is TypeDefinition) {
							return "`" + i;
						}
						throw new NotImplementedException();
					}
				}
			}
			return type.FullName;
		}
	}
}
