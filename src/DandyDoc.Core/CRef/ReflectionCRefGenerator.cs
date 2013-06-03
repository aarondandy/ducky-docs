using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.Reflection;
using DandyDoc.Utility;

namespace DandyDoc.CRef
{

    /// <summary>
    /// Generates a code reference (cref) given a reflected member info.
    /// </summary>
    public class ReflectionCRefGenerator : CRefGeneratorBase
    {

        /// <summary>
        /// A default code reference generator instance.
        /// </summary>
        public static readonly ReflectionCRefGenerator Default = new ReflectionCRefGenerator();

        /// <summary>
        /// A code reference generator that does not append the code reference type prefix.
        /// </summary>
        public static readonly ReflectionCRefGenerator NoPrefix = new ReflectionCRefGenerator(false);

        /// <summary>
        /// A code reference generator that always adds the code reference type prefix.
        /// </summary>
        public static readonly ReflectionCRefGenerator WithPrefix = new ReflectionCRefGenerator(true);

        /// <summary>
        /// A code reference generator that does not append a prefix and performs generic parameter type expansion.
        /// </summary>
        /// <remarks>
        /// This generator is useful when dealing with method parameter types within code references.
        /// </remarks>
        public static readonly ReflectionCRefGenerator NoPrefixForceGenericExpansion = new ReflectionCRefGenerator(false) {
            ForceGenericExpansion = true
        };

        /// <summary>
        /// Creates a code reference generator that will include a cref type prefix.
        /// </summary>
        public ReflectionCRefGenerator()
            : this(true) { }

        /// <summary>
        /// Creates a code reference generator with the desired options.
        /// </summary>
        /// <param name="includeTypePrefix">A flag indicating if generated crefs contain a type prefix.</param>
        public ReflectionCRefGenerator(bool includeTypePrefix)
            : base(includeTypePrefix) {
            ForceGenericExpansion = false;
        }

        /// <summary>
        /// Generic expansion of type parameters will be performed when set.
        /// </summary>
        /// <remarks>
        /// Generic types are encoded different when used as method parameters.
        /// This flag assists with the proper encoding of parameter types.
        /// </remarks>
        protected bool ForceGenericExpansion { get; set; }

        /// <inheritdoc/>
        public override string GetCRef(object entity) {
            if (entity is Assembly)
                return "A:" + ((Assembly)entity).FullName;

            var memberInfo = entity as MemberInfo;
            return memberInfo != null ? GetCRef(memberInfo) : null;
        }

        /// <summary>
        /// Generates a code reference (cref) for the given member info.
        /// </summary>
        /// <param name="info">The member info to create a code reference (cref) for.</param>
        /// <returns>A code reference (cref) for the given member info.</returns>
        public virtual string GetCRef(MemberInfo info) {
            if (info == null) throw new ArgumentNullException("info");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (info is Type)
                return GetCRef((Type)info);

            var type = info.DeclaringType;
            Contract.Assume(null != type);
            var typeCRef = NoPrefix.GetCRef(type);
            Contract.Assume(!String.IsNullOrEmpty(info.Name));
            var memberCRef = info.Name.Replace('.','#');

            char crefTypePrefix = '!';
            var methodBase = info as MethodBase;
            if (null != methodBase) {
                /*if (methodInfo is ConstructorInfo && memberCRef.Length > 1 && memberCRef[0] == '.')
                    memberCRef = '#' + memberCRef.Substring(1);
                else */if (methodBase.IsGenericMethod)
                    memberCRef += String.Concat("``", methodBase.GetGenericArguments().Length);

                var methodParameters = methodBase.GetParameters();
                if (methodParameters.Length > 0)
                    memberCRef += '(' + String.Join(",", methodParameters.Select(GetCRefParamTypeName)) + ')';

                var methodInfo = methodBase as MethodInfo;
                if (methodInfo != null && methodInfo.HasNonVoidReturn() && CSharpOperatorNameSymbolMap.IsConversionOperatorMethodName(methodBase.Name))
                    memberCRef += String.Concat('~', NoPrefixForceGenericExpansion.GetCRef(methodInfo.ReturnType));
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

        private string GetCRef(Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (type.IsGenericParameter)
                return GetGenericParameterName(type);

            return GetFullName(type);
        }

        private string GetGenericParameterName(Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var declaringMethod = type.DeclaringMethod;
            if (null != declaringMethod) {
                return String.Concat("``", type.GenericParameterPosition);
            }
            return String.Concat('`', type.GenericParameterPosition);
        }

        private string GetFullName(Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var typeParts = new List<string>();
            var currentType = type;
            while (true) {
                var currentTypeName = currentType.Name;
                Contract.Assume(!String.IsNullOrEmpty(currentTypeName));
                if (currentType.IsByRef) {
                    // if the type is by ref it should end in a @
                    if (currentTypeName[currentTypeName.Length - 1] == '&') {
                        // .NET seems to add & instead of @ so that may need to get stripped off
                        currentTypeName = currentTypeName.Substring(0, currentTypeName.Length - 1);
                    }
                    currentTypeName = String.Concat(currentTypeName, '@');
                }

                if (currentType.IsGenericType && (ForceGenericExpansion || !currentType.IsGenericTypeDefinition)) {
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
