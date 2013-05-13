using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc.CRef
{
    public class CecilCRefGenerator : CRefGeneratorBase
    {

        public static readonly CecilCRefGenerator Default = new CecilCRefGenerator();

        public static readonly CecilCRefGenerator NoPrefix = new CecilCRefGenerator(false);

        public static readonly CecilCRefGenerator WithPrefix = new CecilCRefGenerator(true);

        public CecilCRefGenerator()
            : this(true) { }

        public CecilCRefGenerator(bool includeTypePrefix)
            : base(includeTypePrefix) { }

        public override string GetCRef(object entity) {
            if (entity is AssemblyDefinition)
                return "A:" + ((AssemblyDefinition)entity).FullName;

            var memberReference = entity as MemberReference;
            return memberReference != null ? GetCRef(memberReference) : null;
        }

        protected virtual string GetCRef(MemberReference reference) {
            if (reference == null) throw new ArgumentNullException("reference");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (reference is TypeReference)
                return GetCRef((TypeReference)reference);

            var type = reference.DeclaringType;
            Contract.Assume(null != type);
            var typeCRef = NoPrefix.GetCRef(type);
            var memberCRef = reference.Name;
            Contract.Assume(!String.IsNullOrEmpty(memberCRef));

            char crefTypePrefix = '\0';
            var methodDefinition = reference as MethodDefinition;
            if (null != methodDefinition) {
                if (methodDefinition.IsConstructor && memberCRef.Length > 1 && memberCRef[0] == '.') {
                    memberCRef = '#' + memberCRef.Substring(1);
                }
                else if (methodDefinition.HasGenericParameters) {
                    memberCRef += String.Concat("``", methodDefinition.GenericParameters.Count);
                }
                if (methodDefinition.HasParameters) {
                    memberCRef += '(' + String.Join(",", methodDefinition.Parameters.Select(GetCRefParamTypeName)) + ')';
                }
                crefTypePrefix = 'M';
            }
            else if (reference is PropertyDefinition) {
                var propertyDefinition = ((PropertyDefinition)reference);
                if (propertyDefinition.HasParameters) {
                    memberCRef += '(' + String.Join(",", propertyDefinition.Parameters.Select(GetCRefParamTypeName)) + ')';
                }
                crefTypePrefix = 'P';
            }
            else if (reference is FieldDefinition) {
                crefTypePrefix = 'F';
            }
            else if (reference is EventDefinition) {
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

        protected virtual string GetCRef(TypeReference reference) {
            if (null == reference) throw new ArgumentNullException("reference");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (reference.IsGenericParameter)
                return GetGenericParameterName((GenericParameter)reference);

            return GetFullName(reference);
        }

        protected virtual string GetGenericParameterName(GenericParameter parameter) {
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

        protected virtual string GetFullName(TypeReference reference) {
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
