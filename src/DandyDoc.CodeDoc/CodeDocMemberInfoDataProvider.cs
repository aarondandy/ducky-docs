using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.ExternalVisibility;
using DandyDoc.Reflection;

namespace DandyDoc.CodeDoc
{
    public class CodeDocMemberInfoProvider<TMember> : CodeDocMemberDataProviderCollection
        where TMember : MemberInfo
    {

        public CodeDocMemberInfoProvider(TMember member) {
            if(member == null) throw new ArgumentNullException("member");
            Contract.EndContractBlock();
            Member = member;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Member != null);
        }

        public TMember Member { get; private set; }

        public override bool? IsPure {
            get {
                if (Member.HasAttribute(t => t.Constructor.Name == "PureAttribute"))
                    return true;
                return base.IsPure;
            }
        }

        public override ExternalVisibilityKind? ExternalVisibility {
            get {
                return Member.GetExternalVisibility();
            }
        }

        public override bool? IsStatic {
            get {
                return Member.IsStatic();
            }
        }

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
