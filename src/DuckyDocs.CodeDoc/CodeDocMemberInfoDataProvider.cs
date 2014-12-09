using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DuckyDocs.ExternalVisibility;
using DuckyDocs.Reflection;

namespace DuckyDocs.CodeDoc
{
    /// <summary>
    /// Provides code doc member attributes for a reflected member info.
    /// </summary>
    /// <typeparam name="TMember">A member info type to provide attributes for.</typeparam>
    public class CodeDocMemberInfoProvider<TMember> : CodeDocMemberDataProviderCollection
        where TMember : MemberInfo
    {
        /// <summary>
        /// Creates a member data provider for the given member.
        /// </summary>
        /// <param name="member">The member to wrap.</param>
        public CodeDocMemberInfoProvider(TMember member) {
            if(member == null) throw new ArgumentNullException("member");
            Contract.EndContractBlock();
            Member = member;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Member != null);
        }

        /// <summary>
        /// The wrapped member.
        /// </summary>
        public TMember Member { get; private set; }

        /// <summary>
        /// Determines if the wrapped member is pure.
        /// </summary>
        public override bool? IsPure {
            get {
                if (Member.HasAttribute(t => t.Constructor.Name == "PureAttribute"))
                    return true;
                return base.IsPure;
            }
        }

        /// <summary>
        /// Calculates the external visibility of the wrapped member.
        /// </summary>
        public override ExternalVisibilityKind? ExternalVisibility {
            get {
                return Member.GetExternalVisibility();
            }
        }

        /// <summary>
        /// Determines if the wrapped member is static.
        /// </summary>
        public override bool? IsStatic {
            get {
                return Member.IsStatic();
            }
        }

        /// <summary>
        /// Determines if this member is obsolete.
        /// </summary>
        public override bool? IsObsolete {
            get {
                return Member.HasAttribute(typeof(ObsoleteAttribute));
            }
        }

        private ParameterInfo GetParameterInfoByName(string parameterName) {
            var methodBase = Member as MethodBase;
            if (methodBase != null)
                return methodBase.GetParameters().FirstOrDefault(x => x.Name == parameterName);
            var propertyInfo = Member as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetIndexParameters().FirstOrDefault(x => x.Name == parameterName);

            var type = Member as Type;
            if (type != null && type.IsDelegateType())
                return type.GetDelegateTypeParameters().FirstOrDefault(x => x.Name == parameterName);

            return null;
        }

        private ParameterInfo GetReturnParameterInfo() {
            var methodInfo = Member as MethodInfo;
            if (methodInfo != null)
                return methodInfo.ReturnParameter;

            var type = Member as Type;
            if (type != null && type.IsDelegateType())
                return type.GetDelegateReturnParameter();

            return null;
        }

        /// <summary>
        /// Determines if the target parameter has null restrictions.
        /// </summary>
        /// <param name="parameterName">The target parameter name.</param>
        /// <returns>A value indicating if the parameter is null restricted.</returns>
        public override bool? RequiresParameterNotEverNull(string parameterName) {
            var parameterInfo = GetParameterInfoByName(parameterName);
            if (parameterInfo != null) {
                var parameterType = parameterInfo.ParameterType;
                if (parameterType.IsValueType && !parameterType.IsNullable()) {
                    return true;
                }

                foreach (var constructorName in parameterInfo.GetCustomAttributesData().Select(attribute => attribute.Constructor.Name)) {
                    if (constructorName == "CanBeNullAttribute")
                        return false;
                    if (constructorName == "NotNullAttribute")
                        return true;
                }
            }
            return base.RequiresParameterNotEverNull(parameterName);
        }

        /// <summary>
        /// Determines if the result has null restrictions.
        /// </summary>
        public override bool? EnsuresResultNotEverNull {
            get {
                var returnParameter = GetReturnParameterInfo();
                if (returnParameter != null) {
                    var parameterType = returnParameter.ParameterType;
                    if (parameterType.IsValueType && !parameterType.IsNullable()) {
                        return true;
                    }
                }

                var constructorNames = Member.GetCustomAttributesData().Select(x => x.Constructor.Name);
                if (returnParameter != null)
                    constructorNames = constructorNames.Concat(returnParameter.GetCustomAttributesData().Select(x => x.Constructor.Name));

                foreach (var constructorName in constructorNames) {
                    if (constructorName == "CanBeNullAttribute")
                        return false;
                    if (constructorName == "NotNullAttribute")
                        return true;
                }
                return base.EnsuresResultNotEverNull;
            }
        }

    }
}
