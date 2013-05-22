using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using DandyDoc.ExternalVisibility;
using Mono.Cecil;
using DandyDoc.Cecil;

namespace DandyDoc.CodeDoc
{
    public class CodeDocMemberReferenceProvider<TMember> : CodeDocMemberDataProviderCollection
        where TMember : MemberReference
    {

        public CodeDocMemberReferenceProvider(TMember member) {
            if(member == null) throw new ArgumentNullException("member");
            Contract.EndContractBlock();
            Member = member;
            Definition = member.ToDefinition();
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Member != null);
        }

        public TMember Member { get; private set; }

        public IMemberDefinition Definition { get; private set; }

        public bool HasDefinition { get { return Definition != null; } }

        public override bool? IsPure {
            get {
                if (HasDefinition && Definition.HasAttribute(t => t.Constructor.Name == "PureAttribute"))
                    return true;
                return base.IsPure;
            }
        }

        public override ExternalVisibilityKind? ExternalVisibility {
            get {
                if (HasDefinition)
                    return Definition.GetExternalVisibility();
                return Member.GetExternalVisibilityOrDefault();
            }
        }

        public override bool? IsStatic {
            get {
                if (HasDefinition)
                    return Definition.IsStatic();
                return base.IsStatic;
            }
        }

        public override bool? IsObsolete {
            get {
                if (HasDefinition && Definition.HasAttribute(t => t.Constructor.Name == "ObsoleteAttribute"))
                    return true;
                return base.IsObsolete;
            }
        }

        private ParameterReference GetParameterReferenceByName(string parameterName) {
            var methodReference = Member as MethodReference;
            if (methodReference != null)
                return methodReference.Parameters.FirstOrDefault(x => x.Name == parameterName);

            var propertyReference = Member as PropertyReference;
            if (propertyReference != null)
                return propertyReference.Parameters.FirstOrDefault(x => x.Name == parameterName);

            if (HasDefinition) {
                var typeDefinition = Definition as TypeDefinition;
                if (typeDefinition != null && typeDefinition.IsDelegateType())
                    return typeDefinition.GetDelegateTypeParameters().FirstOrDefault(x => x.Name == parameterName);
            }
            return null;
        }

        private MethodReturnType GetMethodReturn() {
            var methodReference = Member as MethodReference;
            if (methodReference != null)
                return methodReference.MethodReturnType;

            if (HasDefinition) {
                var type = Definition as TypeDefinition;
                if (type != null && type.IsDelegateType())
                    return type.GetDelegateMethodReturn();
            }
            return null;
        }

        public override bool? RequiresParameterNotEverNull(string parameterName) {
            var parameterReference = GetParameterReferenceByName(parameterName);
            if (parameterReference != null) {
                var parameterTypeReference = parameterReference.ParameterType;
                if (parameterTypeReference.IsValueType && !parameterTypeReference.IsNullable()) {
                    return true;
                }

                var parameterDefinition = parameterReference.ToDefinition();
                if (parameterDefinition != null) {
                    foreach (var constructorName in parameterDefinition.CustomAttributes.Select(attribute => attribute.Constructor.Name)) {
                        if (constructorName == "CanBeNullAttribute")
                            return false;
                        if (constructorName == "NotNullAttribute")
                            return true;
                    }
                }
            }
            return base.RequiresParameterNotEverNull(parameterName);
        }

        public override bool? EnsuresResultNotEverNull {
            get {
                var methodReturn = GetMethodReturn();
                if (methodReturn != null) {
                    var returnTypeReference = methodReturn.ReturnType;
                    if (returnTypeReference.IsValueType && !returnTypeReference.IsNullable()) {
                        return true;
                    }
                }

                var constructorNames = Enumerable.Empty<string>();

                if (HasDefinition)
                    constructorNames = Definition.CustomAttributes.Select(x => x.Constructor.Name);

                if (methodReturn != null)
                    constructorNames = constructorNames.Concat(methodReturn.CustomAttributes.Select(x => x.Constructor.Name));

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
