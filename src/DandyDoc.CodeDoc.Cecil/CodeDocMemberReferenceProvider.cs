using System;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.ExternalVisibility;
using Mono.Cecil;
using DandyDoc.Cecil;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// Provides various documentation properties based on a Cecil member reference and its child providers.
    /// </summary>
    /// <typeparam name="TMember">The member reference to handle.</typeparam>
    public class CodeDocMemberReferenceProvider<TMember> : CodeDocMemberDataProviderCollection
        where TMember : MemberReference
    {

        /// <summary>
        /// Creates a member data provider for the given member.
        /// </summary>
        /// <param name="member"></param>
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

        /// <summary>
        /// The wrapped member.
        /// </summary>
        public TMember Member { get; private set; }

        /// <summary>
        /// The definition of the wrapped member.
        /// </summary>
        public IMemberDefinition Definition { get; private set; }

        /// <summary>
        /// Indicates that the wrapped member has a definition.
        /// </summary>
        public bool HasDefinition { get { return Definition != null; } }

        /// <summary>
        /// Determines if the wrapped member is pure.
        /// </summary>
        public override bool? IsPure {
            get {
                if (HasDefinition && Definition.HasAttribute(t => t.Constructor.Name == "PureAttribute"))
                    return true;
                return base.IsPure;
            }
        }

        /// <summary>
        /// Calculates the external visibility of the wrapped member.
        /// </summary>
        public override ExternalVisibilityKind? ExternalVisibility {
            get {
                if (HasDefinition)
                    return Definition.GetExternalVisibility();
                return Member.GetExternalVisibilityOrDefault();
            }
        }

        /// <summary>
        /// Determines if the wrapped member is static.
        /// </summary>
        public override bool? IsStatic {
            get {
                if (HasDefinition)
                    return Definition.IsStatic();
                return base.IsStatic;
            }
        }

        /// <summary>
        /// Determines if this member is obsolete.
        /// </summary>
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

        /// <summary>
        /// Determines if the target parameter has null restrictions.
        /// </summary>
        /// <param name="parameterName">The target parameter name.</param>
        /// <returns>A value indicating if the parameter is null restricted.</returns>
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

        /// <summary>
        /// Determines if the result has null restrictions.
        /// </summary>
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
