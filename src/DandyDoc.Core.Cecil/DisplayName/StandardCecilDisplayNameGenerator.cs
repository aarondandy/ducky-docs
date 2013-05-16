using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Cecil;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.DisplayName
{

    /// <summary>
    /// Generates display names for Cecil members.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note: the resulting display name can not be resolved back into the
    /// generating declaration or reference as it may be missing critical information.
    /// Use <see cref="DandyDoc.CRef.CecilCRefGenerator"/> if a unique and reversible
    /// identifying name is required.
    /// </para>
    /// </remarks>
    public class StandardCecilDisplayNameGenerator
    {

        /// <summary>
        /// Creates a default display name generator.
        /// </summary>
        public StandardCecilDisplayNameGenerator() {
            IncludeNamespaceForTypes = false;
            ShowGenericParametersOnDefinition = true;
            ShowTypeNameForMembers = false;
            ListSeperator = ", ";
        }

        /// <summary>
        /// Gets a value indicating if namespaces will be included.
        /// </summary>
        public bool IncludeNamespaceForTypes { get; set; }

        /// <summary>
        /// Gets a value indicating if generic parameters will be added to generic members.
        /// </summary>
        public bool ShowGenericParametersOnDefinition { get; set; }

        /// <summary>
        /// Gets a value indicating if declaring types will be added to members.
        /// </summary>
        public bool ShowTypeNameForMembers { get; set; }

        /// <summary>
        /// Gets the list separator.
        /// </summary>
        public string ListSeperator { get; set; }

        /// <summary>
        /// Generates a display name for the given member.
        /// </summary>
        /// <param name="memberReference">The member to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(MemberReference memberReference) {
            if (null == memberReference) throw new ArgumentNullException("memberReference");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            if (memberReference is TypeReference)
                return GetDisplayName((TypeReference)memberReference);
            if (memberReference is MethodDefinition)
                return GetDisplayName((MethodDefinition)memberReference);
            if (memberReference is PropertyDefinition)
                return GetDisplayName((PropertyDefinition)memberReference);
            return GetGenericDisplayName(memberReference);
        }

        /// <summary>
        /// Generates a display name for the given method.
        /// </summary>
        /// <param name="methodDefinition">The method to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(MethodDefinition methodDefinition) {
            if (null == methodDefinition) throw new ArgumentNullException("methodDefinition");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            string name;
            if (methodDefinition.IsConstructor) {
                var typeName = methodDefinition.DeclaringType.Name;
                if (methodDefinition.DeclaringType.HasGenericParameters) {
                    var tickIndex = typeName.LastIndexOf('`');
                    if (tickIndex >= 0)
                        typeName = typeName.Substring(0, tickIndex);
                }
                name = typeName;
            }
            else if (methodDefinition.IsOperatorOverload()) {
                if (CSharpOperatorNameSymbolMap.TryGetOperatorSymbol(methodDefinition.Name, out name)) {
                    name = String.Concat("operator ", name);
                }
                else {
                    name = methodDefinition.Name;
                    if (name.StartsWith("op_"))
                        name = name.Substring(3);
                }
            }
            else {
                name = methodDefinition.Name;
                if (methodDefinition.HasGenericParameters) {
                    var tickIndex = name.LastIndexOf('`');
                    if (tickIndex >= 0)
                        name = name.Substring(0, tickIndex);
                    name = String.Concat(
                        name,
                        '<',
                        String.Join(ListSeperator, methodDefinition.GenericParameters.Select(GetDisplayName)),
                        '>');
                }
            }

            Contract.Assume(methodDefinition.Parameters != null);
            name = String.Concat(name, '(', GetParameterText(methodDefinition.Parameters), ')');

            if (ShowTypeNameForMembers) {
                Contract.Assume(null != methodDefinition.DeclaringType);
                name = String.Concat(GetDisplayName(methodDefinition.DeclaringType), '.', name);
            }

            return name;
        }

        /// <summary>
        /// Generates a display name for the given property.
        /// </summary>
        /// <param name="propertyDefinition">The property to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(PropertyDefinition propertyDefinition) {
            if (null == propertyDefinition) throw new ArgumentNullException("propertyDefinition");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var name = propertyDefinition.Name;
            if (propertyDefinition.HasParameters) {
                char openParen, closeParen;
                if ("Item".Equals(name)) {
                    openParen = '[';
                    closeParen = ']';
                }
                else {
                    openParen = '(';
                    closeParen = ')';
                }
                Contract.Assume(propertyDefinition.Parameters != null);
                name = String.Concat(
                    name,
                    openParen,
                    GetParameterText(propertyDefinition.Parameters),
                    closeParen);
            }
            if (ShowTypeNameForMembers) {
                Contract.Assume(null != propertyDefinition.DeclaringType);
                name = String.Concat(GetDisplayName(propertyDefinition.DeclaringType), '.', name);
            }
            return name;
        }

        /// <summary>
        /// Generates a display name for the given member.
        /// </summary>
        /// <param name="memberDefinition">The member to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(IMemberDefinition memberDefinition) {
            if (null == memberDefinition) throw new ArgumentNullException("memberDefinition");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            Contract.Assume(null != (memberDefinition as MemberReference));
            return GetDisplayName(memberDefinition as MemberReference);
        }

        /// <summary>
        /// Generates a display name for the given type.
        /// </summary>
        /// <param name="typeDefinition">The type to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(TypeDefinition typeDefinition) {
            if (null == typeDefinition) throw new ArgumentNullException("typeDefinition");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return GetDisplayName((TypeReference)typeDefinition);
        }

        private string GetParameterText(IEnumerable<ParameterDefinition> definitions) {
            if (null == definitions) throw new ArgumentNullException("definitions");
            Contract.EndContractBlock();
            return String.Join(ListSeperator, definitions.Select(GetParameterText));
        }

        private string GetParameterText(ParameterDefinition definition) {
            Contract.Requires(null != definition);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            Contract.Assume(definition.ParameterType != null);
            return GetDisplayName(definition.ParameterType);
        }

        private string GetTypeDisplayName(TypeReference reference) {
            Contract.Requires(reference != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var result = reference.Name;
            if (ShowGenericParametersOnDefinition) {
                if (reference is TypeDefinition) {
                    var definition = (TypeDefinition)reference;
                    if (definition.HasGenericParameters) {
                        var tickIndex = result.LastIndexOf('`');
                        if (tickIndex >= 0)
                            result = result.Substring(0, tickIndex);

                        IList<GenericParameter> genericParameters = definition.GenericParameters;
                        if (definition.IsNested && definition.DeclaringType.HasGenericParameters) {
                            var parentGenericParams = definition.DeclaringType.GenericParameters;
                            genericParameters = genericParameters.Where(p => parentGenericParams.All(t => t.Name != p.Name)).ToList();
                        }

                        if (genericParameters.Count > 0) {
                            result = String.Concat(
                                result,
                                '<',
                                String.Join(
                                    ListSeperator,
                                    genericParameters.Select(GetDisplayName)),
                                '>');
                        }
                    }
                }
                else if (reference.IsGenericInstance) {
                    var genericInstanceType = reference as GenericInstanceType;
                    Contract.Assume(null != genericInstanceType);
                    var tickIndex = result.LastIndexOf('`');
                    if (tickIndex >= 0)
                        result = result.Substring(0, tickIndex);

                    result = String.Concat(
                        result,
                        '<',
                        String.Join(
                            ListSeperator,
                            genericInstanceType.GenericArguments.Select(GetDisplayName)),
                        '>');
                }
            }
            return result;
        }

        private string GetDisplayName(TypeReference reference, bool hideParams = false) {
            if (null == reference) throw new ArgumentNullException("reference");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (reference.IsGenericParameter)
                return reference.Name;

            var rootTypeReference = reference;
            string fullTypeName;
            if (ShowTypeNameForMembers) {
                fullTypeName = GetNestedTypeDisplayName(ref rootTypeReference);
            }
            else {
                fullTypeName = GetTypeDisplayName(reference);
                while (rootTypeReference.DeclaringType != null) {
                    rootTypeReference = rootTypeReference.DeclaringType;
                }
            }

            if (IncludeNamespaceForTypes && !String.IsNullOrEmpty(rootTypeReference.Namespace))
                fullTypeName = String.Concat(rootTypeReference.Namespace, '.', fullTypeName);

            var definition = reference as TypeDefinition;
            if (null != definition) {
                if (definition.IsDelegateType() && !hideParams) {
                    fullTypeName = String.Concat(fullTypeName, '(', GetParameterText(definition.GetDelegateTypeParameters()), ')');
                }
            }

            return fullTypeName;
        }

        /// <summary>
        /// Walks up the declaring types while accumulating a nested type name as the result.
        /// </summary>
        /// <param name="typeReference">The type to be walked and mutated.</param>
        /// <returns>The full nested type name.</returns>
        private string GetNestedTypeDisplayName(ref TypeReference typeReference) {
            Contract.Requires(null != typeReference);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var typeParts = new List<string>();
            while (null != typeReference) {
                typeParts.Insert(0, GetTypeDisplayName(typeReference));
                if (!typeReference.IsNested)
                    break;

                Contract.Assume(null != typeReference.DeclaringType);
                typeReference = typeReference.DeclaringType;
            }
            return typeParts.Count == 1
                ? typeParts[0]
                : String.Join(".", typeParts);
        }

        private string GetGenericDisplayName(MemberReference reference) {
            Contract.Requires(null != reference);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var name = reference.Name;
            if (ShowTypeNameForMembers) {
                Contract.Assume(null != reference.DeclaringType);
                name = String.Concat(GetDisplayName(reference.DeclaringType), '.', name);
            }
            return name;
        }

    }
}
