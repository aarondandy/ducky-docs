using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc.Overlays.Cref
{

	/// <summary>
	/// A documentation overlay which can translate between Cecil references and XML documentation cref hyperlinks.
	/// </summary>
	public class CRefOverlay
	{

		private static readonly CRefOverlay DefaultInstance = new CRefOverlay(new AssemblyDefinitionCollection());

		public static string GetDefaultCref(IMemberDefinition definition){
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return DefaultInstance.GetCref(definition);
		}

		public static string GetDefaultCref(MemberReference reference){
			if(null == reference) throw new ArgumentNullException("reference");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return DefaultInstance.GetCref(reference);
		}

		public CRefOverlay(AssemblyDefinitionCollection assemblyDefinitionCollection) {
			if(null == assemblyDefinitionCollection) throw new ArgumentNullException("assemblyDefinitionCollection");
			Contract.EndContractBlock();
			AssemblyDefinitionCollection = assemblyDefinitionCollection;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public MemberReference GetReference(string cref){
			if (String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid cref.", "cref");
			Contract.EndContractBlock();
			var parsedCref = new ParsedCref(cref);
			return GetReference(parsedCref);
		}

		public MemberReference GetReference(ParsedCref cref) {
			if(null == cref) throw new ArgumentNullException("cref");
			Contract.EndContractBlock();
			if (String.IsNullOrEmpty(cref.TargetType))
				return GetTypeDefinition(cref) ?? (GetMemberDefinition(cref) as MemberReference);
			if ("T".Equals(cref.TargetType))
				return GetTypeDefinition(cref);
			return GetMemberDefinition(cref) as MemberReference;
		}

		public TypeDefinition GetTypeDefinition(string cref) {
			if(String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid cref.", "cref");
			Contract.EndContractBlock();
			return GetTypeDefinition(new ParsedCref(cref));
		}

		private TypeDefinition GetTypeDefinition(ParsedCref parsedCref) {
			Contract.Requires(null != parsedCref);
			if (!String.IsNullOrEmpty(parsedCref.TargetType) && !String.Equals("T", parsedCref.TargetType, StringComparison.OrdinalIgnoreCase))
				throw new NotSupportedException("The given CRef type is not supported: " + parsedCref.TargetType);
			if (!String.IsNullOrEmpty(parsedCref.ParamParts))
				return null;
			var typeName = parsedCref.CoreName;
			return String.IsNullOrEmpty(typeName)
				? null
				: AssemblyDefinitionCollection
				.Select(x => GetTypeDefinition(x, typeName))
				.FirstOrDefault(x => null != x);
		}

		private TypeDefinition GetTypeDefinition(AssemblyDefinition assemblyDefinition, string typeName) {
			Contract.Requires(null != assemblyDefinition);
			Contract.Requires(!String.IsNullOrEmpty(typeName));
			Contract.Assume(null != assemblyDefinition.Modules);
			foreach (var type in assemblyDefinition.Modules.SelectMany(x => x.Types)) {
				var typeNamespace = type.Namespace;
				if(typeName.Length <= typeNamespace.Length)
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
			if(String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid cref.", "cref");
			Contract.EndContractBlock();
			return GetMemberDefinition(new ParsedCref(cref));
		}

		public IMemberDefinition GetMemberDefinition(ParsedCref cref) {
			if(null == cref) throw new ArgumentNullException("cref");
			Contract.EndContractBlock();
			if (String.Equals("T", cref.TargetType, StringComparison.OrdinalIgnoreCase))
				return GetTypeDefinition(cref);

			var memberResult = AssemblyDefinitionCollection.Select(x => GetMemberDefinitionExcludingTypes(x, cref)).FirstOrDefault(x => null != x);
			if (null != memberResult)
				return memberResult;

			if (String.IsNullOrEmpty(cref.TargetType))
				return GetTypeDefinition(cref);

			return null;
		}

		private IMemberDefinition GetMemberDefinitionExcludingTypes(AssemblyDefinition assemblyDefinition, ParsedCref parsedCref) {
			Contract.Requires(null != assemblyDefinition);
			Contract.Requires(null != parsedCref);

			var lastDotIndex = parsedCref.CoreName.LastIndexOf('.');
			if (lastDotIndex <= 0 || (parsedCref.CoreName.Length-1) == lastDotIndex)
				return null;

			var typeName = parsedCref.CoreName.Substring(0, lastDotIndex);
			var type = GetTypeDefinition(assemblyDefinition, typeName);
			if (null == type)
				return null;

			var memberName = parsedCref.CoreName.Substring(lastDotIndex+1);
			Contract.Assume(!String.IsNullOrEmpty(memberName));

			if (String.IsNullOrEmpty(parsedCref.TargetType)) {
				var paramTypes = parsedCref.ParamPartTypes;
				return type.Methods.FirstOrDefault(m => MethodMatches(m, memberName, paramTypes))
					?? type.Properties.FirstOrDefault(p => PropertyMatches(p, memberName, paramTypes))
					?? type.Events.FirstOrDefault(e => EventMatches(e, memberName))
					?? (type.Fields.FirstOrDefault(f => FieldMatches(f, memberName)) as IMemberDefinition);
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
				throw new NotSupportedException("The given CRef type is not supported: " + parsedCref.TargetType);
			}
		}

		private bool MethodMatches(MethodDefinition methodDefinition, string nameTest, string[] paramTypeTest){
			Contract.Requires(null != methodDefinition);
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
				else{
					return false;
				}
			}

			return methodDefinition.HasParameters
				? ParametersMatch(methodDefinition.Parameters, paramTypeTest)
				: null == paramTypeTest || paramTypeTest.Length == 0;
		}

		private bool PropertyMatches(PropertyDefinition propertyDefinition, string nameTest, string[] paramTypeTest){
			Contract.Requires(null != propertyDefinition);
			Contract.Requires(!String.IsNullOrEmpty(nameTest));
			if (!nameTest.Equals(propertyDefinition.Name))
				return false;

			return propertyDefinition.HasParameters
				? ParametersMatch(propertyDefinition.Parameters, paramTypeTest)
				: null == paramTypeTest || paramTypeTest.Length == 0;
		}

		private bool ParametersMatch(IList<ParameterDefinition> parameters, string[] paramTypeTest){
			Contract.Requires(null != parameters);
			if (null == paramTypeTest)
				return false;
			if (paramTypeTest.Length != parameters.Count)
				return false;
			for (int i = 0; i < paramTypeTest.Length; i++) {
				Contract.Assume(null != parameters[i].ParameterType);
				if (!String.Equals(paramTypeTest[i], GetCref(parameters[i].ParameterType, true)))
					return false;
			}
			return true;
		}

		private bool FieldMatches(FieldDefinition fieldDefinition, string nameTest) {
			Contract.Requires(null != fieldDefinition);
			Contract.Requires(!String.IsNullOrEmpty(nameTest));
			return nameTest.Equals(fieldDefinition.Name);
		}

		private bool EventMatches(EventDefinition eventDefinition, string nameTest) {
			Contract.Requires(null != eventDefinition);
			Contract.Requires(!String.IsNullOrEmpty(nameTest));
			return nameTest.Equals(eventDefinition.Name);
		}

		public string GetCref(TypeDefinition definition, bool hideCrefType = false) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return GetCref((TypeReference)definition, hideCrefType);
		}

		public string GetCref(TypeReference reference, bool hideCrefType = false) {
			if(null == reference) throw new ArgumentNullException("reference");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			
			if (reference.IsGenericParameter) {
				var genericParameter = (GenericParameter)reference;
				var paramIndex = genericParameter.Owner.GenericParameters.IndexOf(genericParameter);
				return String.Concat(
					genericParameter.Owner is TypeDefinition ? "`" : "``",
					paramIndex
				);
			}

			var typeParts = new List<string>();
			var currentType = reference;
			while (null != currentType){
				var currentTypeName = currentType.Name;
				if (currentType.IsGenericInstance){
					var genericInstanceType = currentType as GenericInstanceType;
					Contract.Assume(null != genericInstanceType);
					var tickIndex = currentTypeName.LastIndexOf('`');
					if (tickIndex >= 0)
						currentTypeName = currentTypeName.Substring(0, tickIndex);
					currentTypeName = String.Concat(
						currentTypeName,
						'{',
						String.Join(",",genericInstanceType.GenericArguments.Select(x => GetCref(x, true))),
						'}'
					);
				}

				typeParts.Insert(0, currentTypeName);
				if (!currentType.IsNested)
					break;

				Contract.Assume(null != currentType.DeclaringType);
				currentType = currentType.DeclaringType;
			}

			var ns = currentType.Namespace;
			var cref = typeParts.Count == 1
				? typeParts[0]
				: String.Join(".", typeParts);
			Contract.Assume(!String.IsNullOrEmpty(cref));

			if (!String.IsNullOrEmpty(ns))
				cref = ns + '.' + cref;

			if (!hideCrefType)
				cref = "T:" + cref;
			return cref;
		}

		private string GetCrefParamTypeName(ParameterDefinition parameterDefinition) {
			Contract.Requires(null != parameterDefinition);
			Contract.Assume(null != parameterDefinition.ParameterType);
			return GetCref(parameterDefinition.ParameterType, true);
		}

		public string GetCref(IMemberDefinition definition, bool hideCrefType = false) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			Contract.Assume(null != (definition as MemberReference));
			return GetCref(definition as MemberReference, hideCrefType);
		}

		/// <summary>
		/// Creates a cref hyperlink for the given member or type.
		/// </summary>
		/// <param name="memberRef">The member or type reference to create a cref hyperlink for.</param>
		/// <param name="hideCrefType">When true the cref target type prefix will be omitted.</param>
		/// <returns>A cref used for hyperlinking.</returns>
		/// <remarks>
		/// <para>
		/// By default all generated cref hyperlinks will have a prefix specifying the type of target
		/// the cref points to. Use the <paramref name="hideCrefType"/> parameter to control this behavior.
		/// </para>
		/// </remarks>
		public string GetCref(MemberReference memberRef, bool hideCrefType = false) {
			if(null == memberRef) throw new ArgumentNullException("memberRef");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

			if (memberRef is TypeReference)
				return GetCref((TypeReference)memberRef, hideCrefType);

			var type = memberRef.DeclaringType;
			Contract.Assume(null != type);
			var typeCref = GetCref(type, true);
			var memberCref = memberRef.Name;
			Contract.Assume(!String.IsNullOrEmpty(memberCref));

			char crefTypePrefix = '\0';
			var methodDefinition = memberRef as MethodDefinition;
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
			else if (memberRef is PropertyDefinition) {
				var propertyDefinition = ((PropertyDefinition)memberRef);
				if (propertyDefinition.HasParameters) {
					memberCref += '(' + String.Join(",", propertyDefinition.Parameters.Select(GetCrefParamTypeName)) + ')';
				}
				crefTypePrefix = 'P';
			}
			else if (memberRef is FieldDefinition) {
				crefTypePrefix = 'F';
			}
			else if (memberRef is EventDefinition) {
				crefTypePrefix = 'E';
			}

			var cref = typeCref + '.' + memberCref;
			if (!hideCrefType && crefTypePrefix != '\0')
				cref = String.Concat(crefTypePrefix, ':', cref);

			return cref;
		}

		public string GetCref(MethodDefinition definition, bool hideCrefType = false) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return GetCref((MemberReference)definition, hideCrefType);
		}

		public string GetCref(PropertyDefinition definition, bool hideCrefType = false) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return GetCref((MemberReference)definition, hideCrefType);
		}

		public string GetCref(FieldDefinition definition, bool hideCrefType = false) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return GetCref((MemberReference)definition, hideCrefType);
		}

		public string GetCref(EventDefinition definition, bool hideCrefType = false) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return GetCref((MemberReference)definition, hideCrefType);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != AssemblyDefinitionCollection);
		}

	}
}
