using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.CRef;
using DandyDoc.DisplayName;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class ReflectionCodeDocEntityRepository : ICodeDocEntityRepository
    {

        protected static readonly StandardReflectionDisplayNameGenerator RegularTypeDisplayNameOverlay = new StandardReflectionDisplayNameGenerator {
            ShowTypeNameForMembers = false
        };

        protected static readonly StandardReflectionDisplayNameGenerator NestedTypeDisplayNameOverlay = new StandardReflectionDisplayNameGenerator {
            ShowTypeNameForMembers = true
        };

        protected static readonly StandardReflectionDisplayNameGenerator FullTypeDisplayNameOverlay = new StandardReflectionDisplayNameGenerator {
            ShowTypeNameForMembers = true,
            IncludeNamespaceForTypes = true
        };

        private static CRefIdentifier GetCRefIdentifier(MemberInfo memberInfo){
            Contract.Requires(memberInfo != null);
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
            return new CRefIdentifier(ReflectionCRefGenerator.WithPrefix.GetCRef(memberInfo));
        }

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

        public ICodeDocEntityContent GetEntity(string cRef) {
            if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
            Contract.EndContractBlock();
            var memberInfo = CRefLookup.GetMember(cRef);
            return memberInfo == null ? null : ConvertToEntity(memberInfo);
        }

        public ICodeDocEntityContent GetEntity(CRefIdentifier cRef) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            var memberInfo = CRefLookup.GetMember(cRef);
            return memberInfo == null ? null : ConvertToEntity(memberInfo);
        }

        protected virtual ICodeDocEntityContent ConvertToEntity(MemberInfo memberInfo) {
            if(memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocEntity>() != null);

            if (memberInfo is Type){
                return ConvertToTypeEntity((Type)memberInfo);
            }

            throw new NotImplementedException();
        }

        private CodeDocType ConvertToTypeEntity(Type type){
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);
            var result = new CodeDocType(GetCRefIdentifier(type));
            ApplyStandardXmlDocs(result, result.CRef.FullCRef);
            ApplyTypeAttributes(result, type);
            return result;
        }

        private void ApplyStandardXmlDocs(CodeDocEntityContentBase model, string cRef){
            Contract.Requires(model != null);
            Contract.Requires(!String.IsNullOrEmpty(cRef));
            model.XmlDocs = XmlDocs.GetMember(cRef);
        }

        private void ApplyTypeAttributes(CodeDocType model, Type type){
            Contract.Requires(model != null);
            Contract.Requires(type != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(type);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(type);
            model.Title = model.ShortName;
            model.NamespaceName = type.Namespace;

            if (type.IsEnum)
                model.SubTitle = "Enumeration";
            else if (type.IsValueType)
                model.SubTitle = "Structure";
            else if (type.IsInterface)
                model.SubTitle = "Interface";
            else if (type.IsDelegateType())
                model.SubTitle = "Delegate";
            else
                model.SubTitle = "Class";

            ; // TODO: details of the type
        }

    }
}
