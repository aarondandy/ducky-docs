using System;
using System.Diagnostics.Contracts;
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

    }
}
