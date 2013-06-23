using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
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
            //var msdnRepository = new MsdnCodeDocMemberRepository();
            var cecilRepository = new CecilMemberRepository();
            var supportingRepositories = new CodeDocMergedMemberRepository(
                //msdnRepository,
                cecilRepository);
            var targetRepository = new ReflectionCodeDocMemberRepository(
                new ReflectionCRefLookup(
                    typeof (ReflectionCRefLookup).Assembly,
                    typeof (ICodeDocMemberRepository).Assembly,
                    Assembly.ReflectionOnlyLoadFrom(HostingEnvironment.MapPath("~/bin/DandyDoc.Core.Cecil.dll")),
                    Assembly.ReflectionOnlyLoadFrom(HostingEnvironment.MapPath("~/bin/DandyDoc.CodeDoc.Cecil.dll")),
                    Assembly.ReflectionOnlyLoadFrom(HostingEnvironment.MapPath("~/bin/TestLibrary1.dll"))
                    ),
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.Core.XML")),
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.Core.Cecil.XML")),
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.CodeDoc.XML")),
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.CodeDoc.Cecil.XML")),
                new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/TestLibrary1.XML"))
            );

            var allRepositories = new CodeDocMergedMemberRepository(
                targetRepository,
                supportingRepositories);

            TargetRepository = targetRepository;
            AllRepositories = allRepositories;
        }

        public ICodeDocMemberRepository TargetRepository { get; private set; }

        public ICodeDocMemberRepository AllRepositories { get; private set; }

        public ICodeDocMember GetModel(string cRef) {
            if (String.IsNullOrEmpty(cRef))
                return null;
            return GetModel(new CRefIdentifier(cRef));
        }

        public ICodeDocMember GetModel(CRefIdentifier cRef) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            return new CodeDocRepositorySearchContext(AllRepositories).Search(cRef);
        }

    }
}