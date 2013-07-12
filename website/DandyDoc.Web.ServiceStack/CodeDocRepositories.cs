using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;
using DandyDoc.XmlDoc;

namespace DandyDoc.Web.ServiceStack
{
    public class CodeDocRepositories
    {

        static CodeDocRepositories() {
            // this is needed so reflection only load can find the other assemblies
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyResolveEventHandler;
        }

        private static Assembly ReflectionOnlyResolveEventHandler(object sender, ResolveEventArgs args) {
            var assemblyName = new AssemblyName(args.Name);
            var binPath = HostingEnvironment.MapPath(String.Format("~/bin/{0}.dll", assemblyName.Name));
            if (File.Exists(binPath))
                return Assembly.ReflectionOnlyLoadFrom(binPath);
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        public CodeDocRepositories() {
            var msdnRepository = new CodeDocRepositoryFailureProtectionWrapper(new MsdnCodeDocMemberRepository(), new TimeSpan(0,0,10));
            var cecilRepository = new CecilMemberRepository();
            SupportingRepository = new CodeDocMergedMemberRepository(
                msdnRepository,
                cecilRepository);

            TargetRepository = new ReflectionCodeDocMemberRepository(
                new ReflectionCRefLookup(
                    typeof (ReflectionCRefLookup).Assembly,
                    typeof (ICodeDocMemberRepository).Assembly,
                    Assembly.ReflectionOnlyLoadFrom(HostingEnvironment.MapPath("~/bin/DandyDoc.Core.Cecil.dll")),
                    Assembly.ReflectionOnlyLoadFrom(HostingEnvironment.MapPath("~/bin/DandyDoc.CodeDoc.Cecil.dll"))
                ),
                // the /bin/bin folder is used due to an odd publish quirk
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/bin/DandyDoc.Core.XML")),
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/bin/DandyDoc.Core.Cecil.XML")),
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/bin/DandyDoc.CodeDoc.XML")),
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/bin/DandyDoc.CodeDoc.Cecil.XML"))
            );

        }

        public ICodeDocMemberRepository TargetRepository { get; private set; }

        public ICodeDocMemberRepository SupportingRepository { get; private set; }

        public ICodeDocMember GetModelFromTarget(CRefIdentifier cRef, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            return new CodeDocRepositorySearchContext(new[] { TargetRepository, SupportingRepository }, detailLevel)
                .CloneWithOneUnvisited(TargetRepository)
                .Search(cRef);
        }

        public ICodeDocMember GetModelFromAny(CRefIdentifier cRef, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            return new CodeDocRepositorySearchContext(new[] { TargetRepository, SupportingRepository }, detailLevel).Search(cRef);
        }

    }
}