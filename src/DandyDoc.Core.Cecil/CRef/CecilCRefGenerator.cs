using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Cecil;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.CRef
{

    /// <summary>
    /// Generates a code reference (cref) given a Cecil member reference.
    /// </summary>
    public class CecilCRefGenerator : CRefGeneratorBase
    {

        /// <summary>
        /// A default code reference generator.
        /// </summary>
        public static readonly CecilCRefGenerator Default = new CecilCRefGenerator();

        /// <summary>
        /// A code reference generator that does not add a code reference type prefix.
        /// </summary>
        public static readonly CecilCRefGenerator NoPrefix = new CecilCRefGenerator(false);

        /// <summary>
        /// A code reference generator that always adds a code reference type prefix.
        /// </summary>
        public static readonly CecilCRefGenerator WithPrefix = new CecilCRefGenerator(true);

        /// <summary>
        /// A code reference generator that creates generic definition code references.
        /// </summary>
        public static readonly CecilCRefGenerator WithPrefixGenericDefinition = new CecilCRefGenerator(true) {
            ForceGenericDefinition = true
        };

        /// <summary>
        /// Creates a code reference generator that will include a cref type prefix.
        /// </summary>
        public CecilCRefGenerator()
            : this(true) { }

        /// <summary>
        /// Creates a code reference generator with the desired options.
        /// </summary>
        /// <param name="includeTypePrefix">A flag indicating if generated crefs contain a type prefix.</param>
        public CecilCRefGenerator(bool includeTypePrefix)
            : base(includeTypePrefix) {
            ForceGenericDefinition = false;
        }

        private CecilCRefGenerator Clone() {
            return new CecilCRefGenerator(IncludeTypePrefix) {
                ForceGenericDefinition = ForceGenericDefinition
            };
        }

        private CecilCRefGenerator WithoutPrefix() {
            if (!IncludeTypePrefix)
                return this;

            var clone = this.Clone();
            clone.IncludeTypePrefix = false;
            return clone;
        }

        /// <summary>
        /// Indicates if generation of a generic definition should be forced.
        /// </summary>
        protected bool ForceGenericDefinition { get; set; }

        /// <inheritdoc/>
        public override string GetCRef(object entity) {
            if (entity is AssemblyDefinition)
                return "A:" + ((AssemblyDefinition)entity).FullName;

            var memberReference = entity as MemberReference;
            return memberReference != null ? GetCRef(memberReference) : null;
        }

        /// <summary>
        /// Generates a code reference (cref) for the given member reference.
        /// </summary>
        /// <param name="reference">The member reference to create a code reference (cref) for.</param>
        /// <returns>A code reference (cref) for the given member reference.</returns>
        public virtual string GetCRef(MemberReference reference) {
            if (reference == null) throw new ArgumentNullException("reference");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (reference is TypeReference)
                return GetCRef((TypeReference)reference);

            var type = reference.DeclaringType;
            Contract.Assume(null != type);
            var typeCRef = WithoutPrefix().GetCRef(type);
            var memberCRef = reference.Name.Replace('.','#');
            Contract.Assume(!String.IsNullOrEmpty(memberCRef));

            char crefTypePrefix = '\0';
            var methodReference = reference as MethodReference;
            if (null != methodReference) {
                //var methodDefinition = reference as MethodDefinition;
                if (methodReference.HasGenericParameters)
                    memberCRef += String.Concat("``", methodReference.GenericParameters.Count);

                if (methodReference.HasParameters)
                    memberCRef += '(' + String.Join(",", methodReference.Parameters.Select(GetCRefParamTypeName)) + ')';

                if (methodReference.HasNonVoidReturn() && CSharpOperatorNameSymbolMap.IsConversionOperatorMethodName(methodReference.Name))
                    memberCRef += String.Concat('~', NoPrefix.GetCRef(methodReference.ReturnType));

                crefTypePrefix = 'M';
            }
            else if (reference is PropertyReference) {
                var propertyReference = reference as PropertyReference;
                var propertyDefinition = propertyReference.ToDefinition();
                if (propertyDefinition != null && propertyDefinition.HasParameters)
                    memberCRef += '(' + String.Join(",", propertyDefinition.Parameters.Select(GetCRefParamTypeName)) + ')';

                crefTypePrefix = 'P';
            }
            else if (reference is FieldReference) {
                crefTypePrefix = 'F';
            }
            else if (reference is EventReference) {
                crefTypePrefix = 'E';
            }

            var cref = typeCRef + '.' + memberCRef;
            if (IncludeTypePrefix && crefTypePrefix != '\0')
                cref = String.Concat(crefTypePrefix, ':', cref);

            return cref;
        }

        private string GetCRefParamTypeName(ParameterDefinition parameterDefinition) {
            Contract.Requires(parameterDefinition != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            Contract.Assume(null != parameterDefinition.ParameterType);
            return NoPrefix.GetCRef(parameterDefinition.ParameterType);
        }

        private string GetCRef(TypeReference reference) {
            if (null == reference) throw new ArgumentNullException("reference");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (reference.IsGenericParameter)
                return GetGenericParameterName((GenericParameter)reference);

            return GetFullName(reference);
        }

        private string GetGenericParameterName(GenericParameter parameter) {
            if (parameter == null) throw new ArgumentNullException("parameter");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var paramIndex = parameter.Owner.GenericParameters.IndexOf(parameter);
            if (paramIndex < 0)
                return parameter.Name;
            return String.Concat(
                parameter.Owner is TypeDefinition ? "`" : "``",
                paramIndex
            );
        }

        private string GetFullName(TypeReference reference) {
            if (null == reference) throw new ArgumentNullException("reference");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var typeParts = new List<string>();
            var currentType = reference;
            while (true) {
                var currentTypeName = currentType.Name;
                Contract.Assume(!String.IsNullOrEmpty(currentTypeName));
                if (currentType.IsByReference) {
                    // if the type is by ref it should end in a @
                    if (currentTypeName[currentTypeName.Length - 1] == '&') {
                        // .NET seems to add & instead of @ so that may need to get stripped off
                        currentTypeName = currentTypeName.Substring(0, currentTypeName.Length - 1);
                    }
                    currentTypeName = String.Concat(currentTypeName, '@');
                }

                if (currentType.IsGenericInstance) {
                    var genericInstanceType = currentType as GenericInstanceType;
                    Contract.Assume(null != genericInstanceType);
                    var tickIndex = currentTypeName.LastIndexOf('`');
                    if (tickIndex >= 0)
                        currentTypeName = currentTypeName.Substring(0, tickIndex);

                    currentTypeName = String.Concat(
                        currentTypeName,
                        '{',
                        String.Join(",", genericInstanceType.GenericArguments.Select(NoPrefix.GetCRef)),
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
