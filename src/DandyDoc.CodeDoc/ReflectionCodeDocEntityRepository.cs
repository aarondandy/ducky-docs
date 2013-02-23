using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class ReflectionCodeDocEntityRepository : ICodeDocEntityRepository
    {

        public ReflectionCodeDocEntityRepository(ReflectionCRefLookup cRefLookup)
            : this(cRefLookup, null)
        {
            Contract.Requires(cRefLookup != null);
        }

        public ReflectionCodeDocEntityRepository(ReflectionCRefLookup cRefLookup, IEnumerable<XmlAssemblyDocumentation> xmlDocs) {
            if(cRefLookup == null) throw new ArgumentNullException("cRefLookup");
            Contract.EndContractBlock();
            CRefLookup = cRefLookup;
            XmlDocs = new XmlAssemblyDocumentationCollection(xmlDocs);
        }

        public XmlAssemblyDocumentationCollection XmlDocs { get; private set; }

        public ReflectionCRefLookup CRefLookup { get; private set; }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(CRefLookup != null);
            Contract.Invariant(XmlDocs != null);
        }

        public ICodeDocEntity GetEntity(string cRef) {
            if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid", "cRef");
            Contract.EndContractBlock();
            var memberInfo = CRefLookup.GetMember(cRef);
            return memberInfo == null ? null : ConvertToEntity(memberInfo);
        }

        public ICodeDocEntity GetEntity(CRefIdentifier cRef) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            var memberInfo = CRefLookup.GetMember(cRef);
            return memberInfo == null ? null : ConvertToEntity(memberInfo);
        }

        protected virtual ICodeDocEntity ConvertToEntity(MemberInfo memberInfo) {
            if(memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocEntity>() != null);
            throw new NotImplementedException();
        }

    }
}
