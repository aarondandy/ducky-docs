﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.Cref
{
	public class CrefOverlay
	{

		public CrefOverlay(AssemblyDefinitionCollection assemblyDefinitionCollection) {
			if(null == assemblyDefinitionCollection) throw new ArgumentNullException("assemblyDefinitionCollection");
			Contract.EndContractBlock();
			AssemblyDefinitionCollection = assemblyDefinitionCollection;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public TypeDefinition GetTypeDefinition(string cref) {
			var parsedCref = ParsedCref.Parse(cref);
			if (!String.IsNullOrEmpty(parsedCref.TargetType) && !String.Equals("T", parsedCref.TargetType, StringComparison.OrdinalIgnoreCase))
				return null;
			if (!String.IsNullOrEmpty(parsedCref.ParamParts))
				return null;
			var typeName = parsedCref.CoreName;
			if (String.IsNullOrEmpty(typeName))
				return null;
			return AssemblyDefinitionCollection
				.Select(x => GetTypeDefinition(x, typeName))
				.FirstOrDefault(x => null != x);
		}

		private TypeDefinition GetTypeDefinition(AssemblyDefinition assemblyDefinition, string typeName) {
			Contract.Requires(null != assemblyDefinition);
			Contract.Requires(!String.IsNullOrEmpty(typeName));

			foreach (var type in assemblyDefinition.Modules.SelectMany(x => x.Types)) {
				var typeNamespace = type.Namespace;
				if(typeName.Length <= typeNamespace.Length)
					continue;
				if (!typeName.StartsWith(typeNamespace))
					continue;
				if(typeName[typeNamespace.Length] != '.')
					continue;
				var typeOnly = typeName.Substring(typeNamespace.Length + 1);
				Contract.Assume(!String.IsNullOrEmpty(typeOnly));
				var result = ResolveTypeByName(type, typeOnly);
				if (null != result)
					return result;
			}
			return null;
		}

		private TypeDefinition ResolveTypeByName(TypeDefinition type, string typeName) {
			Contract.Requires(null != type);
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

			foreach (var nestedType in type.NestedTypes) {
				var result = ResolveTypeByName(nestedType, otherNamePart);
				if (null != result)
					return result;
			}
			return null;
		}

		public IMemberDefinition GetMemberDefinition(string cref) {
			var parsedCref = ParsedCref.Parse(cref);
			if (!String.IsNullOrEmpty(parsedCref.TargetType) && String.Equals("T", parsedCref.TargetType, StringComparison.OrdinalIgnoreCase))
				return null;
			return AssemblyDefinitionCollection.Select(x => GetMemberDefinition(x, parsedCref)).FirstOrDefault(x => null != x);
		}

		private IMemberDefinition GetMemberDefinition(AssemblyDefinition assemblyDefinition, ParsedCref parsedCref) {
			Contract.Requires(null != assemblyDefinition);
			Contract.Requires(null != parsedCref);

			var lastDotIndex = parsedCref.CoreName.LastIndexOf('.');
			if (lastDotIndex <= 0)
				return null;

			var typeName = parsedCref.CoreName.Substring(0, lastDotIndex);
			var type = GetTypeDefinition(assemblyDefinition, typeName);
			if (null == type)
				return null;

			var memberName = parsedCref.CoreName.Substring(lastDotIndex+1);

			if (String.IsNullOrEmpty(parsedCref.TargetType)) {
				var paramTypes = parsedCref.ParamPartTypes;
				return type.Methods.FirstOrDefault(m => MethodMatches(m, memberName, paramTypes))
					?? type.Properties.FirstOrDefault(p => PropertyMatches(p, memberName, paramTypes))
					?? type.Fields.FirstOrDefault(f => FieldMatches(f, memberName))
					?? (type.Events.FirstOrDefault(e => EventMatches(e, memberName)) as IMemberDefinition);
			}
			else if ("M".Equals(parsedCref.TargetType, StringComparison.OrdinalIgnoreCase)) {
				var paramTypes = parsedCref.ParamPartTypes;
				return type.Methods.FirstOrDefault(m => MethodMatches(m, memberName, paramTypes));
			}
			else if ("P".Equals(parsedCref.TargetType, StringComparison.OrdinalIgnoreCase)) {
				var paramTypes = parsedCref.ParamPartTypes;
				return type.Properties.FirstOrDefault(p => PropertyMatches(p, memberName, paramTypes));
			}
			else if ("F".Equals(parsedCref.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.Fields.FirstOrDefault(f => FieldMatches(f, memberName));
			}
			else if ("E".Equals(parsedCref.TargetType, StringComparison.OrdinalIgnoreCase)) {
				return type.Events.FirstOrDefault(e => EventMatches(e, memberName));
			}
			else {
				throw new NotSupportedException();
			}
		}

		private bool MethodMatches(MethodDefinition methodDefinition, string nameTest, string[] paramTypeTest) {
			if (nameTest[0] == '#' && methodDefinition.Name[0] == '.') {
				if (nameTest.Length != methodDefinition.Name.Length)
					return false;
				if (!nameTest.Substring(1).Equals(methodDefinition.Name.Substring(1)))
					return false;
			}
			else if (!nameTest.Equals(methodDefinition.Name)) {
				if (methodDefinition.HasGenericParameters && nameTest.StartsWith(methodDefinition.Name)) {
					var methodGenericParamCount = methodDefinition.Parameters
						.Where(x => x.ParameterType.IsGenericParameter)
						.Select(x => x.ParameterType as GenericParameter)
						.Count(x => x.Owner == methodDefinition);
					if (!nameTest.Equals(methodDefinition.Name + "``" + methodGenericParamCount)) {
						return false;
					}
				}
				else{
					return false;
				}
			}

			if (methodDefinition.HasParameters) {
				if (null == paramTypeTest)
					return false;
				var methodParams = methodDefinition.Parameters;
				if (paramTypeTest.Length != methodParams.Count)
					return false;
				for (int i = 0; i < paramTypeTest.Length; i++) {
					var methodParamType = methodParams[i].ParameterType;
					var methodParamCref = GetCref(methodParamType, true);
					if (!String.Equals(paramTypeTest[i], methodParamCref))
						return false;
				}
				return true;
			}
			else {
				return null == paramTypeTest || paramTypeTest.Length == 0;
			}
		}

		private bool PropertyMatches(PropertyDefinition propertyDefinition, string nameTest, string[] paramTypeTest) {
			if (!nameTest.Equals(propertyDefinition.Name))
				return false;
			if (propertyDefinition.HasParameters) {
				if (null == paramTypeTest)
					return false;
				var propertyParams = propertyDefinition.Parameters;
				if (paramTypeTest.Length != propertyParams.Count)
					return false;
				for (int i = 0; i < paramTypeTest.Length; i++) {
					var propertyParamType = propertyParams[i].ParameterType;
					if (!String.Equals(paramTypeTest[i], propertyParamType.FullName))
						return false;
				}
				return true;
			}
			else {
				return null == paramTypeTest || paramTypeTest.Length == 0;
			}
		}

		private bool FieldMatches(FieldDefinition fieldDefinition, string nameTest) {
			return nameTest.Equals(fieldDefinition.Name);
		}

		private bool EventMatches(EventDefinition eventDefinition, string nameTest) {
			return nameTest.Equals(eventDefinition.Name);
		}

		public string GetCref(TypeDefinition typeDef, bool hideCrefType = false){
			return GetCref((TypeReference) typeDef, hideCrefType);
		}

		public string GetCref(TypeReference typeRef, bool hideCrefType = false) {
			string cref;
			if (typeRef.IsGenericParameter) {
				//var cref = typeRef.Name;
				var genericParameter = typeRef as GenericParameter;
				if (null != genericParameter){
					var paramIndex = genericParameter.Owner.GenericParameters.IndexOf(genericParameter);
					cref = String.Concat(
						genericParameter.Owner is TypeDefinition ? "`" : "``",
						paramIndex);

					if (!hideCrefType)
						cref = "G:" + cref;
					return cref;
				}
			}

			var typeParts = new List<string>();
			TypeReference currentType = typeRef;
			while (null != currentType){
				string currentTypeName = currentType.Name;
				if (currentType.IsGenericInstance){
					var genericInstanceType = currentType as GenericInstanceType;
					Contract.Assume(null != genericInstanceType);
					var tickIndex = currentTypeName.LastIndexOf('`');
					if (tickIndex >= 0)
						currentTypeName = currentTypeName.Substring(0, tickIndex);
					currentTypeName += String.Concat(
						'{',
						String.Join(",",genericInstanceType.GenericArguments.Select(x => GetCref(x, true))),
						'}'
					);
				}

				typeParts.Insert(0, currentTypeName);
				if (currentType.IsNested) {
					Contract.Assume(null != currentType.DeclaringType);
					currentType = currentType.DeclaringType;
				}
				else {
					break;
				}
			}

			var ns = currentType.Namespace;
			cref = String.Join(".", typeParts);

			if (!String.IsNullOrEmpty(ns))
				cref = ns + '.' + cref;

			if (!hideCrefType)
				cref = "T:" + cref;
			return cref;
		}

		private string GetCrefParamTypeName(ParameterDefinition parameterDefinition) {
			Contract.Requires(null != parameterDefinition);
			return GetCref(parameterDefinition.ParameterType, true);
		}

		public string GetCref(IMemberDefinition memberDef, bool hideCrefType = false) {
			if(null == memberDef) throw new ArgumentNullException("memberDef");
			Contract.EndContractBlock();
			var type = memberDef.DeclaringType;
			if (null == type) {
				throw new InvalidOperationException("The given member has an invalid declaring type.");
			}
			var typeCref = GetCref(type, true);
			var memberCref = memberDef.Name;
			if(String.IsNullOrEmpty(memberCref))
				throw new InvalidOperationException("The given member has an invalid name.");

			char crefTypePrefix;

			var methodDefinition = memberDef as MethodDefinition;
			if (null != methodDefinition) {
				if (methodDefinition.IsConstructor && memberCref.Length > 1 && memberCref[0] == '.') {
					memberCref = '#' + memberCref.Substring(1);
				}
				else if (methodDefinition.HasGenericParameters){
					memberCref += String.Concat("``", methodDefinition.GenericParameters.Count);
				}
				if (methodDefinition.HasParameters) {
					memberCref += '(' + String.Join(",", methodDefinition.Parameters.Select(GetCrefParamTypeName)) + ')';
				}
				crefTypePrefix = 'M';
			}
			else if (memberDef is PropertyDefinition) {
				var propertyDefinition = ((PropertyDefinition)memberDef);
				if (propertyDefinition.HasParameters) {
					memberCref += '(' + String.Join(",", propertyDefinition.Parameters.Select(GetCrefParamTypeName)) + ')';
				}
				crefTypePrefix = 'P';
			}
			else if (memberDef is FieldDefinition) {
				crefTypePrefix = 'F';
			}
			else if (memberDef is EventDefinition) {
				crefTypePrefix = 'E';
			}
			else {
				throw new NotImplementedException();
			}

			var cref = typeCref + '.' + memberCref;
			if (!hideCrefType)
				cref = String.Concat(crefTypePrefix, ':', cref);

			return cref;
		}

	}
}
