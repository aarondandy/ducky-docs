using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.Cecil
{
    public static class CecilUtilities
    {

        private static readonly ReadOnlyCollection<ParameterDefinition> EmptyParameterDefinitionCollection = Array.AsReadOnly(new ParameterDefinition[0]);

        public static string GetFilePath(this AssemblyDefinition assembly) {
            if (assembly == null) throw new ArgumentNullException("assembly");
            Contract.EndContractBlock();
            Contract.Assume(assembly.MainModule.FullyQualifiedName != null);
            return new FileInfo(assembly.MainModule.FullyQualifiedName).FullName;
        }

        public static string GetFileName(this AssemblyDefinition assembly) {
            if(assembly == null) throw new ArgumentNullException("assembly");
            Contract.Assume(assembly.MainModule.FullyQualifiedName != null);
            return new FileInfo(assembly.MainModule.FullyQualifiedName).Name;
        }

        public static bool IsLiteral(this FieldReference fieldReference) {
            Contract.Requires(fieldReference != null);
            var definition = fieldReference.ToDefinition();
            return definition != null && definition.IsLiteral;
        }

        public static bool IsInitOnly(this FieldReference fieldReference) {
            Contract.Requires(fieldReference != null);
            var definition = fieldReference.ToDefinition();
            return definition != null && definition.IsInitOnly;
        }

        public static bool IsOperatorOverload(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            var definition = methodReference.ToDefinition();
            return definition != null && definition.IsOperatorOverload();
        }

        public static bool IsOperatorOverload(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            if (!methodDefinition.IsStatic)
                return false;
            Contract.Assume(!String.IsNullOrEmpty(methodDefinition.Name));
            return CSharpOperatorNameSymbolMap.IsOperatorName(methodDefinition.Name);
        }

        public static bool IsExtensionMethod(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            if (!methodReference.HasParameters)
                return false;
            var definition = methodReference.ToDefinition();
            return definition != null && definition.IsExtensionMethod();
        }

        public static bool IsExtensionMethod(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            return methodDefinition.HasParameters
                && methodDefinition.Parameters.Count > 0
                && methodDefinition.HasCustomAttributes
                && methodDefinition.HasAttribute(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute");

        }

        public static bool IsOverride(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            var definition = methodReference.ToDefinition();
            return definition != null && definition.IsOverride();
        }

        public static bool IsOverride(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            return methodDefinition.IsVirtual && methodDefinition.IsReuseSlot;
        }

        public static bool IsSealed(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            var definition = methodReference.ToDefinition();
            return definition != null && definition.IsSealed();
        }

        public static bool IsSealed(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            return methodDefinition.IsFinal && methodDefinition.IsOverride();
        }

        public static bool IsItemIndexerProperty(this PropertyReference propertyReference) {
            Contract.Requires(propertyReference != null);
            return "Item".Equals(propertyReference.Name)
                && propertyReference.Parameters.Count > 0;
        }

        public static bool IsConstructor(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            var definition = methodReference.ToDefinition();
            return definition != null && definition.IsConstructor;
        }

        public static bool IsDelegateType(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            var definition = typeReference.ToDefinition();
            return definition != null && definition.IsDelegateType();
        }

        public static bool IsDelegateType(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            var baseType = typeDefinition.BaseType;
            if (baseType == null)
                return false;

            if (!baseType.FullName.Equals("System.MulticastDelegate"))
                return false;

            return typeDefinition.HasMethods && typeDefinition.Methods.Any(x => "Invoke".Equals(x.Name));
        }

        public static IList<ParameterDefinition> GetDelegateTypeParameters(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<IList<ParameterDefinition>>() != null);
            if (!IsDelegateType(typeDefinition))
                return EmptyParameterDefinitionCollection;

            Contract.Assume(typeDefinition.Methods != null);
            var method = typeDefinition.Methods.FirstOrDefault(x => "Invoke".Equals(x.Name));
            return null == method || !method.HasParameters
                ? (IList<ParameterDefinition>)EmptyParameterDefinitionCollection
                : method.Parameters;
        }

        public static TypeReference GetDelegateReturnType(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            Contract.Requires(typeDefinition.IsDelegateType());
            var method = typeDefinition
                .GetAllMethods(x => "Invoke".Equals(x.Name))
                .FirstOrDefault();
            return method == null ? null : method.ReturnType;
        }

        public static bool IsFinalizer(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            var methodDefinition = methodReference.ToDefinition();
            return methodDefinition != null && methodDefinition.IsFinalizer();
        }

        public static bool IsFinalizer(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            return !methodDefinition.IsStatic
                && !methodDefinition.HasParameters
                && "Finalize".Equals(methodDefinition.Name);
        }

        public static bool IsStatic(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            var methodDefinition = methodReference.ToDefinition();
            return methodDefinition != null && methodDefinition.IsStatic;
        }

        public static bool IsStatic(this FieldReference fieldReference) {
            Contract.Requires(fieldReference != null);
            var fieldDefinition = fieldReference.ToDefinition();
            return fieldDefinition != null && fieldDefinition.IsStatic;
        }

        public static bool IsStatic(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            var typeDefinition = typeReference.ToDefinition();
            return typeDefinition != null && typeDefinition.IsStatic();
        }

        public static bool IsStatic(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            return typeDefinition.IsAbstract && typeDefinition.IsSealed;
        }

        public static bool IsStatic(this PropertyReference propertyReference) {
            Contract.Requires(propertyReference != null);
            var propertyDefinition = propertyReference.ToDefinition();
            return propertyDefinition != null && propertyDefinition.IsStatic();
        }

        public static bool IsStatic(this PropertyDefinition propertyDefinition) {
            Contract.Requires(propertyDefinition != null);
            var method = propertyDefinition.GetMethod ?? propertyDefinition.SetMethod;
            return method != null && method.IsStatic;
        }

        public static bool IsStatic(this EventReference eventReference) {
            Contract.Requires(eventReference != null);
            var eventDefinition = eventReference.ToDefinition();
            return eventDefinition != null && eventDefinition.IsStatic();
        }

        public static bool IsStatic(this EventDefinition eventDefinition) {
            Contract.Requires(eventDefinition != null);
            var method = eventDefinition.AddMethod ?? eventDefinition.InvokeMethod;
            return method != null && method.IsStatic;
        }

        public static bool IsStatic(this MemberReference memberReference) {
            Contract.Requires(memberReference != null);
            if (memberReference is MethodReference)
                return ((MethodReference)memberReference).IsStatic();
            if (memberReference is FieldReference)
                return ((FieldReference)memberReference).IsStatic();
            if (memberReference is TypeReference)
                return ((TypeReference)memberReference).IsStatic();
            if (memberReference is PropertyReference)
                return ((PropertyReference)memberReference).IsStatic();
            if (memberReference is EventReference)
                return ((EventReference)memberReference).IsStatic();
            throw new NotSupportedException();
        }

        public static bool IsStatic(this IMemberDefinition memberDefinition) {
            Contract.Requires(memberDefinition != null);
            if (memberDefinition is MethodDefinition)
                return ((MethodDefinition)memberDefinition).IsStatic;
            if (memberDefinition is FieldDefinition)
                return ((FieldDefinition)memberDefinition).IsStatic;
            if (memberDefinition is TypeDefinition)
                return ((TypeDefinition)memberDefinition).IsStatic();
            if (memberDefinition is PropertyDefinition)
                return ((PropertyDefinition)memberDefinition).IsStatic();
            if (memberDefinition is EventDefinition)
                return ((EventDefinition)memberDefinition).IsStatic();
            throw new NotSupportedException();
        }

        public static bool IsVoid(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            return String.Equals(typeReference.FullName, "System.Void");
        }

        public static bool IsEnum(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            var typeDefinition = typeReference.ToDefinition();
            return typeDefinition != null && typeDefinition.IsEnum;
        }

        public static bool IsInterface(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            var typeDefinition = typeReference.ToDefinition();
            return typeDefinition != null && typeDefinition.IsInterface;
        }

        public static bool HasAttribute(this IMemberDefinition memberDefinition, Func<CustomAttribute, bool> predicate) {
            Contract.Requires(memberDefinition != null);
            Contract.Requires(predicate != null);
            return memberDefinition.HasCustomAttributes
                && memberDefinition.CustomAttributes.Any(predicate);
        }

        public static TypeDefinition ToDefinition(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            return typeReference as TypeDefinition ?? (typeReference.Resolve());
        }

        public static MethodDefinition ToDefinition(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            return methodReference as MethodDefinition ?? (methodReference.Resolve());
        }

        public static FieldDefinition ToDefinition(this FieldReference fieldReference) {
            Contract.Requires(fieldReference != null);
            return fieldReference as FieldDefinition ?? (fieldReference.Resolve());
        }

        public static PropertyDefinition ToDefinition(this PropertyReference propertyReference) {
            Contract.Requires(propertyReference != null);
            return propertyReference as PropertyDefinition ?? (propertyReference.Resolve());
        }

        public static EventDefinition ToDefinition(this EventReference eventReference) {
            Contract.Requires(eventReference != null);
            return eventReference as EventDefinition ?? (eventReference.Resolve());
        }

        public static ParameterDefinition ToDefinition(this ParameterReference parameterReference) {
            Contract.Requires(parameterReference != null);
            return parameterReference as ParameterDefinition ?? (parameterReference.Resolve());
        }

        public static IMemberDefinition ToDefinition(this MemberReference memberReference) {
            var definition = memberReference as IMemberDefinition;
            if (definition != null)
                return definition;
            if (memberReference is TypeReference)
                return ((TypeReference)memberReference).ToDefinition();
            if (memberReference is MethodReference)
                return ((MethodReference)memberReference).ToDefinition();
            if (memberReference is FieldReference)
                return ((FieldReference)memberReference).ToDefinition();
            if (memberReference is PropertyReference)
                return ((PropertyReference)memberReference).ToDefinition();
            if (memberReference is EventReference)
                return ((EventReference)memberReference).ToDefinition();

            return null;
        }

        public static IEnumerable<TypeDefinition> GetBaseChainDefinitions(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            var baseRef = typeDefinition.BaseType;
            while (baseRef != null) {
                typeDefinition = baseRef.ToDefinition();
                if (typeDefinition == null) {
                    yield break;
                }
                yield return typeDefinition;
                baseRef = typeDefinition.BaseType;
            }
        }

        public static IEnumerable<TypeDefinition> GetBaseChainDefinitions(this TypeDefinition typeDefinition, Func<TypeDefinition, bool> predicate) {
            Contract.Requires(typeDefinition != null);
            Contract.Requires(predicate != null);
            return typeDefinition.GetBaseChainDefinitions().Where(predicate);
        }

        public static List<EventDefinition> GetAllEvents(this TypeDefinition typeDefinition, Func<EventDefinition, bool> filter, bool skipInheritance = false) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<EventDefinition>>() != null);
            return typeDefinition.GetAllMembers(
                d => d.HasEvents,
                d => d.Events,
                filter,
                skipInheritance);
        }

        public static List<FieldDefinition> GetAllFields(this TypeDefinition typeDefinition, Func<FieldDefinition, bool> filter = null, bool skipInheritance = false) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<FieldDefinition>>() != null);
            return typeDefinition.GetAllMembers(
                d => d.HasFields,
                d => d.Fields,
                filter,
                skipInheritance);
        }

        public static List<MethodDefinition> GetAllMethods(this TypeDefinition typeDefinition, Func<MethodDefinition, bool> filter = null, bool skipInheritance = false) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<MethodDefinition>>() != null);
            return typeDefinition.GetAllMembers(
                d => d.HasMethods,
                d => d.Methods,
                filter,
                skipInheritance);
        }

        public static List<PropertyDefinition> GetAllProperties(this TypeDefinition typeDefinition, Func<PropertyDefinition, bool> filter = null, bool skipInheritance = false) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<PropertyDefinition>>() != null);
            return typeDefinition.GetAllMembers(
                d => d.HasProperties,
                d => d.Properties,
                filter,
                skipInheritance);
        }

        private static List<TResult> GetAllMembers<TResult>(
            this TypeDefinition typeDefinition,
            Func<TypeDefinition, bool> hasMembers,
            Func<TypeDefinition, IEnumerable<TResult>> getMembers,
            Func<TResult, bool> memberFilter = null,
            bool skipInheritance = false
        )
            where TResult : MemberReference
        {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<TResult>>() != null);

            List<TResult> results;
            if (hasMembers(typeDefinition)) {
                var members = getMembers(typeDefinition);
                Contract.Assume(members != null);
                if (memberFilter != null)
                    members = members.Where(memberFilter);
                results = members.ToList();
            }
            else {
                results = new List<TResult>();
            }

            if (skipInheritance)
                return results;

            var nameSet = new HashSet<string>(results.Select(definition => definition.Name));
            Func<TResult, bool> addIfNotFound = definition => nameSet.Add(definition.Name);
            foreach (var baseTypeDefinition in typeDefinition.GetBaseChainDefinitions(hasMembers)) {
                var members = getMembers(baseTypeDefinition).Where(addIfNotFound);
                if (memberFilter != null)
                    members = members.Where(memberFilter);
                results.AddRange(members);
            }
            return results;
        }

        public static TypeReference GetOuterType(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            Contract.Ensures(Contract.Result<TypeReference>() != null);
            if (typeReference.IsNested) {
                Contract.Assume(typeReference.DeclaringType != null);
                typeReference = typeReference.DeclaringType;
            }
            return typeReference;
        }

    }
}
