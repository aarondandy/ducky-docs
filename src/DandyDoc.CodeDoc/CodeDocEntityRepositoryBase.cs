using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.ExternalVisibility;

namespace DandyDoc.CodeDoc
{
    public abstract class CodeDocEntityRepositoryBase : ICodeDocEntityRepository
    {

        public IList<ICodeDocAssembly> Assemblies { get; protected set; }

        public IList<ICodeDocNamespace> Namespaces { get; protected set; }

        public virtual ICodeDocEntityContent GetContentEntity(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetContentEntity(new CRefIdentifier(cRef));
        }

        public abstract ICodeDocEntityContent GetContentEntity(CRefIdentifier cRef);

        public ICodeDocEntity GetSimpleEntity(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetSimpleEntity(new CRefIdentifier(cRef));
        }

        public abstract ICodeDocEntity GetSimpleEntity(CRefIdentifier cRef);

        protected ICodeDocNamespace GetCodeDocNamespace(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            return Namespaces.FirstOrDefault(x => cRef.Equals(x.CRef));
        }

        protected ICodeDocEntity GetCodeDocNamespaceByName(string namespaceName) {
            return Namespaces.FirstOrDefault(x => x.FullName == namespaceName)
                ?? CreateNamespaceFromName(namespaceName);
        }

        protected ICodeDocEntity CreateNamespaceFromName(string namespaceName) {
            string namespaceFriendlyName;
            if (String.IsNullOrWhiteSpace(namespaceName)) {
                namespaceName = String.Empty;
                namespaceFriendlyName = "global";
            }
            else {
                namespaceFriendlyName = namespaceName;
            }
            return new CodeDocSimpleEntity(new CRefIdentifier("N:" + namespaceName)) {
                FullName = namespaceFriendlyName,
                ShortName = namespaceFriendlyName,
                SubTitle = "Namespace",
                Title = namespaceFriendlyName,
                ExternalVisibility = ExternalVisibilityKind.Public
            };
        }

        protected ICodeDocAssembly GetCodeDocAssembly(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            return Assemblies.FirstOrDefault(x => cRef.Equals(x.CRef))
                ?? Assemblies.FirstOrDefault(x => cRef.CoreName == x.AssemblyFileName)
                ?? Assemblies.FirstOrDefault(x => cRef.CoreName == x.ShortName);
        }

        protected ICodeDocEntity CreateSimpleEntityTypePlaceholder(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            Contract.Ensures(Contract.Result<ICodeDocEntity>() != null);
            var cRefFullName = cRef.CoreName;
            return new CodeDocSimpleEntity(cRef) {
                ShortName = cRefFullName,
                Title = cRefFullName,
                SubTitle = "Type",
                FullName = cRefFullName,
                NamespaceName = String.Empty,
                ExternalVisibility = ExternalVisibilityKind.Public
            };
        }

    }
}
