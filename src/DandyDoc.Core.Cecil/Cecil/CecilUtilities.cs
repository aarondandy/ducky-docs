﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.Cecil
{

    /// <summary>
    /// Various helper methods and extension methods related to Cecil.
    /// </summary>
    public static class CecilUtilities
    {

        private static readonly ParameterDefinition[] EmptyParameterDefinitionCollection = new ParameterDefinition[0];

        /// <summary>
        /// Gets the full file path for a given assembly definition.
        /// </summary>
        /// <param name="assembly">The assembly to locate.</param>
        /// <returns>The assembly file path.</returns>
        public static string GetFilePath(this AssemblyDefinition assembly) {
            if (assembly == null) throw new ArgumentNullException("assembly");
            Contract.EndContractBlock();
            Contract.Assume(assembly.MainModule.FullyQualifiedName != null);
            return new FileInfo(assembly.MainModule.FullyQualifiedName).FullName;
        }

        /// <summary>
        /// Determines if the method definition is an operator overload.
        /// </summary>
        /// <param name="methodDefinition">The method definition to test.</param>
        /// <returns><c>true</c> if the method is an operator overload.</returns>
        public static bool IsOperatorOverload(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            if (!methodDefinition.IsStatic)
                return false;
            Contract.Assume(!String.IsNullOrEmpty(methodDefinition.Name));
            return CSharpOperatorNameSymbolMap.IsOperatorName(methodDefinition.Name);
        }

        /// <summary>
        /// Determines if the method definition is an extension method.
        /// </summary>
        /// <param name="methodDefinition">The method definition to test.</param>
        /// <returns><c>true</c> if the method is an extension method.</returns>
        public static bool IsExtensionMethod(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            return methodDefinition.HasParameters
                && methodDefinition.Parameters.Count > 0
                && methodDefinition.HasCustomAttributes
                && methodDefinition.HasAttribute(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute");
        }

        /// <summary>
        /// Determines if the method is an override of a base method.
        /// </summary>
        /// <param name="methodDefinition">The method definition to test.</param>
        /// <returns><c>true</c> if the method is an override.</returns>
        public static bool IsOverride(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            return methodDefinition.IsVirtual && methodDefinition.IsReuseSlot;
        }

        /// <summary>
        /// Determines if a method definition is sealed.
        /// </summary>
        /// <param name="methodDefinition">The method definition to test.</param>
        /// <returns><c>true</c> if the method is sealed.</returns>
        public static bool IsSealed(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            return methodDefinition.IsFinal && methodDefinition.IsOverride();
        }

        /// <summary>
        /// Determines if a property is an item indexer.
        /// </summary>
        /// <param name="propertyReference">The property to test.</param>
        /// <returns><c>true</c> when the property is an item indexer.</returns>
        public static bool IsItemIndexerProperty(this PropertyReference propertyReference) {
            Contract.Requires(propertyReference != null);
            return "Item".Equals(propertyReference.Name)
                && propertyReference.Parameters.Count > 0;
        }

        /// <summary>
        /// Determines if a type is a delegate type.
        /// </summary>
        /// <param name="typeDefinition">The type definition to test.</param>
        /// <returns><c>true</c> when the type is a delegate.</returns>
        public static bool IsDelegateType(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            var baseType = typeDefinition.BaseType;
            if (baseType == null)
                return false;

            if (!baseType.FullName.Equals("System.MulticastDelegate"))
                return false;

            return typeDefinition.HasMethods && typeDefinition.Methods.Any(x => "Invoke".Equals(x.Name));
        }

        /// <summary>
        /// Extracts the parameters for a delegate type from the invoke method.
        /// </summary>
        /// <param name="typeDefinition">The delegate type to extract parameters from.</param>
        /// <returns>The parameters for the delegate.</returns>
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

        /// <summary>
        /// Extracts the return type for a delegate type from the invoke method.
        /// </summary>
        /// <param name="typeDefinition">The delegate type to extract a return type from.</param>
        /// <returns>The return type.</returns>
        public static TypeReference GetDelegateReturnType(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            Contract.Requires(typeDefinition.IsDelegateType());
            var method = typeDefinition
                .GetAllMethods(x => "Invoke".Equals(x.Name))
                .FirstOrDefault();
            return method == null ? null : method.ReturnType;
        }

        /// <summary>
        /// Extracts the method return type (similar to a paremter definition) for the delegate.
        /// </summary>
        /// <param name="typeDefinition">The delegate to get return information for.</param>
        /// <returns>A method return type for the delegate.</returns>
        public static MethodReturnType GetDelegateMethodReturn(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            Contract.Requires(typeDefinition.IsDelegateType());
            var method = typeDefinition
                .GetAllMethods(x => "Invoke".Equals(x.Name))
                .FirstOrDefault();
            return method == null ? null : method.MethodReturnType;
        }

        /// <summary>
        /// Determines if a method is a finalizer method.
        /// </summary>
        /// <param name="methodDefinition">The method to test.</param>
        /// <returns><c>true</c> when the method is a finalizer.</returns>
        public static bool IsFinalizer(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            return !methodDefinition.IsStatic
                && !methodDefinition.HasParameters
                && "Finalize".Equals(methodDefinition.Name);
        }

        /// <summary>
        /// Determines if a type is static.
        /// </summary>
        /// <param name="typeDefinition">The type to test.</param>
        /// <returns><c>true</c> if the type is static.</returns>
        /// <remarks>
        /// A type is static when it is both abstract and sealed.
        /// </remarks>
        public static bool IsStatic(this TypeDefinition typeDefinition) {
            Contract.Requires(typeDefinition != null);
            return typeDefinition.IsAbstract && typeDefinition.IsSealed;
        }

        /// <summary>
        /// Determines if a property is static.
        /// </summary>
        /// <param name="propertyDefinition">The property to test.</param>
        /// <returns><c>true</c> if the property is static.</returns>
        public static bool IsStatic(this PropertyDefinition propertyDefinition) {
            Contract.Requires(propertyDefinition != null);
            var method = propertyDefinition.GetMethod ?? propertyDefinition.SetMethod;
            return method != null && method.IsStatic;
        }

        /// <summary>
        /// Determines if an event is static.
        /// </summary>
        /// <param name="eventDefinition">The event to test.</param>
        /// <returns><c>true</c> if the event is static.</returns>
        public static bool IsStatic(this EventDefinition eventDefinition) {
            Contract.Requires(eventDefinition != null);
            var method = eventDefinition.AddMethod ?? eventDefinition.InvokeMethod;
            return method != null && method.IsStatic;
        }

        /// <summary>
        /// Determines if a member is static.
        /// </summary>
        /// <param name="memberDefinition">The member to test.</param>
        /// <returns><c>true</c> if the member is static.</returns>
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

        /// <summary>
        /// Determines if a type reference is the void type.
        /// </summary>
        /// <param name="typeReference">The type to test.</param>
        /// <returns><c>true</c> when the type is the void type.</returns>
        public static bool IsVoid(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            return String.Equals(typeReference.FullName, "System.Void");
        }

        /// <summary>
        /// Determines if any attribute on a member satisfies a predicate.
        /// </summary>
        /// <param name="memberDefinition">The member to search for attributes on.</param>
        /// <param name="predicate">The test to apply to all attributes.</param>
        /// <returns><c>true</c> if any attribute satisfies the <paramref name="predicate"/>.</returns>
        public static bool HasAttribute(this IMemberDefinition memberDefinition, Func<CustomAttribute, bool> predicate) {
            Contract.Requires(memberDefinition != null);
            Contract.Requires(predicate != null);
            return memberDefinition.HasCustomAttributes
                && memberDefinition.CustomAttributes.Any(predicate);
        }

        /// <summary>
        /// Attempts to cast or resolve a definition from a reference.
        /// </summary>
        /// <param name="typeReference">The reference to get a definition for.</param>
        /// <returns>A definition or null.</returns>
        public static TypeDefinition ToDefinition(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            return typeReference as TypeDefinition ?? (typeReference.Resolve());
        }

        /// <summary>
        /// Attempts to cast or resolve a definition from a reference.
        /// </summary>
        /// <param name="methodReference">The reference to get a definition for.</param>
        /// <returns>A definition or null.</returns>
        public static MethodDefinition ToDefinition(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            return methodReference as MethodDefinition ?? (methodReference.Resolve());
        }

        /// <summary>
        /// Attempts to cast or resolve a definition from a reference.
        /// </summary>
        /// <param name="fieldReference">The reference to get a definition for.</param>
        /// <returns>A definition or null.</returns>
        public static FieldDefinition ToDefinition(this FieldReference fieldReference) {
            Contract.Requires(fieldReference != null);
            return fieldReference as FieldDefinition ?? (fieldReference.Resolve());
        }

        /// <summary>
        /// Attempts to cast or resolve a definition from a reference.
        /// </summary>
        /// <param name="propertyReference">The reference to get a definition for.</param>
        /// <returns>A definition or null.</returns>
        public static PropertyDefinition ToDefinition(this PropertyReference propertyReference) {
            Contract.Requires(propertyReference != null);
            return propertyReference as PropertyDefinition ?? (propertyReference.Resolve());
        }

        /// <summary>
        /// Attempts to cast or resolve a definition from a reference.
        /// </summary>
        /// <param name="eventReference">The reference to get a definition for.</param>
        /// <returns>A definition or null.</returns>
        public static EventDefinition ToDefinition(this EventReference eventReference) {
            Contract.Requires(eventReference != null);
            return eventReference as EventDefinition ?? (eventReference.Resolve());
        }

        /// <summary>
        /// Attempts to cast or resolve a definition from a reference.
        /// </summary>
        /// <param name="parameterReference">The reference to get a definition for.</param>
        /// <returns>A definition or null.</returns>
        public static ParameterDefinition ToDefinition(this ParameterReference parameterReference) {
            Contract.Requires(parameterReference != null);
            return parameterReference as ParameterDefinition ?? (parameterReference.Resolve());
        }

        /// <summary>
        /// Attempts to cast or resolve a definition from a reference.
        /// </summary>
        /// <param name="memberReference">The reference to get a definition for.</param>
        /// <returns>A definition or null.</returns>
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

        /// <summary>
        /// Gets the base type definitions for a type.
        /// </summary>
        /// <param name="typeDefinition">The type to find the base chain of.</param>
        /// <returns>The base type that the given type inherits from.</returns>
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

        /// <summary>
        /// Gets all event definitions for a type.
        /// </summary>
        /// <param name="typeDefinition">The type to get members for.</param>
        /// <param name="filter">An optional filter for the members.</param>
        /// <param name="skipInheritance">Indicates if inherited types should be checked for members.</param>
        /// <returns>Members that are found for the type.</returns>
        public static List<EventDefinition> GetAllEvents(this TypeDefinition typeDefinition, Func<EventDefinition, bool> filter = null, bool skipInheritance = false, bool inheritStatic = false) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<EventDefinition>>() != null);
            return typeDefinition.GetAllMembers(
                d => d.HasEvents,
                d => d.Events,
                filter,
                skipInheritance,
                inheritStatic);
        }

        /// <summary>
        /// Gets all field definitions for a type.
        /// </summary>
        /// <param name="typeDefinition">The type to get members for.</param>
        /// <param name="filter">An optional filter for the members.</param>
        /// <param name="skipInheritance">Indicates if inherited types should be checked for members.</param>
        /// <returns>Members that are found for the type.</returns>
        public static List<FieldDefinition> GetAllFields(this TypeDefinition typeDefinition, Func<FieldDefinition, bool> filter = null, bool skipInheritance = false, bool inheritStatic = false) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<FieldDefinition>>() != null);
            return typeDefinition.GetAllMembers(
                d => d.HasFields,
                d => d.Fields,
                filter,
                skipInheritance,
                inheritStatic);
        }

        /// <summary>
        /// Gets all method definitions for a type.
        /// </summary>
        /// <param name="typeDefinition">The type to get members for.</param>
        /// <param name="filter">An optional filter for the members.</param>
        /// <param name="skipInheritance">Indicates if inherited types should be checked for members.</param>
        /// <returns>Members that are found for the type.</returns>
        public static List<MethodDefinition> GetAllMethods(this TypeDefinition typeDefinition, Func<MethodDefinition, bool> filter = null, bool skipInheritance = false, bool inheritStatic = false) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<MethodDefinition>>() != null);
            return typeDefinition.GetAllMembers(
                d => d.HasMethods,
                d => d.Methods,
                filter,
                skipInheritance,
                inheritStatic);
        }

        /// <summary>
        /// Gets all property definitions for a type.
        /// </summary>
        /// <param name="typeDefinition">The type to get members for.</param>
        /// <param name="filter">An optional filter for the members.</param>
        /// <param name="skipInheritance">Indicates if inherited types should be checked for members.</param>
        /// <returns>Members that are found for the type.</returns>
        public static List<PropertyDefinition> GetAllProperties(this TypeDefinition typeDefinition, Func<PropertyDefinition, bool> filter = null, bool skipInheritance = false, bool inheritStatic = false) {
            Contract.Requires(typeDefinition != null);
            Contract.Ensures(Contract.Result<List<PropertyDefinition>>() != null);
            return typeDefinition.GetAllMembers(
                d => d.HasProperties,
                d => d.Properties,
                filter,
                skipInheritance,
                inheritStatic);
        }

        private static List<TResult> GetAllMembers<TResult>(
            this TypeDefinition typeDefinition,
            Func<TypeDefinition, bool> hasMembers,
            Func<TypeDefinition, IEnumerable<TResult>> getMembers,
            Func<TResult, bool> memberFilter = null,
            bool skipInheritance = false,
            bool inheritStatic = false
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
            foreach (var baseTypeDefinition in typeDefinition.GetBaseChainDefinitions().Where(hasMembers)) {
                var members = getMembers(baseTypeDefinition).Where(addIfNotFound);
                if (!inheritStatic)
                    members = members.Where(r => {
                        var d = r.ToDefinition();
                        return d == null || !d.IsStatic();
                    });

                if (memberFilter != null)
                    members = members.Where(memberFilter);
                results.AddRange(members);
            }
            return results;
        }

        /// <summary>
        /// Follows the declaring type chain to find the non-nested type from the given type.
        /// </summary>
        /// <param name="typeReference">The type to find a non-nested declaring type for.</param>
        /// <returns>A non-nested declaring type.</returns>
        public static TypeReference GetOuterType(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            Contract.Ensures(Contract.Result<TypeReference>() != null);
            if (typeReference.IsNested) {
                typeReference = typeReference.DeclaringType;
                Contract.Assume(typeReference != null);
            }
            return typeReference;
        }

        /// <summary>
        /// Determines if a type reference is a nullable type.
        /// </summary>
        /// <param name="typeReference">The type reference to test.</param>
        /// <returns><c>true</c> when the given type is nullable.</returns>
        /// <remarks>
        /// Nullable is not to be confused with a reference type.
        /// </remarks>
        public static bool IsNullable(this TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            return typeReference.IsValueType
                && typeReference.IsGenericInstance
                && typeReference.Name == "Nullable`1"
                && typeReference.Namespace == "System";
        }

        public static PropertyDefinition FindNextAncestor(this PropertyDefinition propertyDefinition) {
            Contract.Requires(propertyDefinition != null);
            var propertyName = propertyDefinition.Name;
            var ancestorMethods = new[] {propertyDefinition.GetMethod, propertyDefinition.SetMethod}
                .Where(x => x != null)
                .Select(FindNextAncestor)
                .Where(a => a != null);
            foreach (var ancestor in ancestorMethods) {
                var baseTypeDefinition = ancestor.DeclaringType;
                Contract.Assume(baseTypeDefinition != null);
                var basePropertyDefinition = baseTypeDefinition.Properties
                    .FirstOrDefault(p => p.Name == propertyName);
                if (MembersAreEqual(basePropertyDefinition.GetMethod, ancestor) || MembersAreEqual(basePropertyDefinition.SetMethod, ancestor))
                    return basePropertyDefinition;
            }
            return null;
        }

        public static EventDefinition FindNextAncestor(this EventDefinition eventDefinition) {
            Contract.Requires(eventDefinition != null);
            var eventName = eventDefinition.Name;
            var invokeMethod = eventDefinition.InvokeMethod;
            if (invokeMethod == null)
                return null; // nothing we can work with

            var ancestorMethod = invokeMethod.FindNextAncestor();
            if (ancestorMethod != null) {
                var baseEventDefinition = ancestorMethod.DeclaringType.Events
                    .FirstOrDefault(e => e.Name == eventName);
                if (baseEventDefinition != null && !MembersAreEqual(baseEventDefinition, eventDefinition) && MembersAreEqual(baseEventDefinition.InvokeMethod, ancestorMethod))
                    return baseEventDefinition;
            }
            return null;
        }

        private static bool MembersAreEqual(MemberReference a, MemberReference b) {
            if (a == b)
                return true;
            if (a == null)
                return b == null;
            if (b == null)
                return false;
            return a.FullName == b.FullName;
        }

        private static bool ParameterTypesAreEqual(TypeReference[] a, TypeReference[] b) {
            Contract.Requires(a != null);
            Contract.Requires(b != null);
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++) {
                var ta = a[i];
                var tb = b[i];
                if (MembersAreEqual(ta,tb))
                    return true;
            }
            return false;
        }

        private static MethodDefinition FindMatchingMethod(TypeDefinition typeDefinition, string methodName, TypeReference[] parameterTypes) {
            return typeDefinition.Methods.FirstOrDefault(m =>
                m.Name == methodName
                && ParameterTypesAreEqual(parameterTypes, m.Parameters.Select(p => p.ParameterType).ToArray()));
        }

        public static MethodDefinition FindNextAncestor(this MethodDefinition methodDefinition) {
            Contract.Requires(methodDefinition != null);
            var methodName = methodDefinition.Name;
            var methodParameterTypes = methodDefinition.Parameters.Select(p => p.ParameterType).ToArray();
            var baseTypeDefinition = methodDefinition.DeclaringType.BaseType == null
                ? null
                : methodDefinition.DeclaringType.BaseType.ToDefinition();
            while (baseTypeDefinition != null) {
                var matchingMethod = FindMatchingMethod(baseTypeDefinition, methodName, methodParameterTypes);
                if (matchingMethod != null) {
                    if (MembersAreEqual(matchingMethod.DeclaringType, baseTypeDefinition))
                        return matchingMethod;

                    // skip ahead
                    baseTypeDefinition = matchingMethod.DeclaringType;
                }
                else {
                    if (baseTypeDefinition.BaseType == null)
                        break;
                    baseTypeDefinition = baseTypeDefinition.BaseType.ToDefinition();
                }
            }

            if (methodDefinition.DeclaringType.IsInterface)
                return null;

            var interfaceDefinitions = methodDefinition.DeclaringType.Interfaces
                .Select(x => x.ToDefinition())
                .Where(x => x != null);
            foreach (var interfaceTypeReference in interfaceDefinitions) {
                var matchingMethod = FindMatchingMethod(interfaceTypeReference, methodName, methodParameterTypes);
                if (matchingMethod != null)
                    return matchingMethod;
            }

            return null;
        }

        /// <summary>
        /// Determines if a method has a non-void return type.
        /// </summary>
        /// <param name="methodReference">The method to test.</param>
        /// <returns><c>true</c> when the method returns a non-void type.</returns>
        public static bool HasNonVoidReturn(this MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            return methodReference.ReturnType != null && !methodReference.ReturnType.IsVoid();
        }

    }
}
