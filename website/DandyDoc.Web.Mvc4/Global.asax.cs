using System;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;
using DandyDoc.XmlDoc;
using Mono.Cecil;
using Ninject;
using Ninject.Web.Common;

namespace DandyDoc.Web.Mvc4
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : NinjectHttpApplication
    {

        public class CodeDocRepositories
        {

            public CodeDocRepositories(ICodeDocMemberRepository targetRepository, ICodeDocMemberRepository supportingRepository) {
                TargetRepository = targetRepository;
                SupportingRepository = supportingRepository;
            }

            public ICodeDocMemberRepository TargetRepository { get; private set; }

            public ICodeDocMemberRepository SupportingRepository { get; private set; }

            public CodeDocRepositorySearchContext CreateSearchContext() {
                return new CodeDocRepositorySearchContext(new[] { TargetRepository, SupportingRepository });
            }
        }

        public class CecilMemberRepository : ReflectionCodeDocMemberRepository
        {

            protected class MemberGenerator : ReflectionCodeDocMemberRepository.MemberGenerator
            {
                public MemberGenerator(ReflectionCodeDocMemberRepository repository, CodeDocRepositorySearchContext searchContext)
                    :base(repository, searchContext) { }

                protected override Uri GetUri(CRefIdentifier cRef, MemberInfo memberInfo) {
                    if (memberInfo != null) {
                        var type = memberInfo as Type ?? memberInfo.DeclaringType;
                        if (type != null) {
                            var namespaceName = type.Namespace;
                            var typeName = type.Name;
                            return new Uri(String.Format("https://github.com/jbevain/cecil/blob/master/{0}/{1}.cs", namespaceName, typeName), UriKind.Absolute);
                        }
                    }
                    return base.GetUri(cRef, memberInfo);
                }
            }

            public CecilMemberRepository() : base(new ReflectionCRefLookup(typeof(AssemblyDefinition).Assembly)) { }

            protected override MemberGeneratorBase CreateGenerator(CodeDocRepositorySearchContext searchContext) {
                return new MemberGenerator(this, searchContext);
            }

        }

        protected override IKernel CreateKernel() {
            var kernel = new StandardKernel();

            kernel.Bind<CodeDocRepositories>().ToMethod(x => {
                var targetRepository = new ReflectionCodeDocMemberRepository(
                    new ReflectionCRefLookup(
                        typeof(ReflectionCRefLookup).Assembly,
                        typeof(ICodeDocMemberRepository).Assembly,
                        typeof(CecilCRefLookup).Assembly,
                        typeof(CecilCodeDocMemberRepository).Assembly,
                        Assembly.ReflectionOnlyLoadFrom(HostingEnvironment.MapPath("~/bin/TestLibrary1.dll"))
                    ),
                    new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.Core.XML")),
                    new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.Core.Cecil.XML")),
                    new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.CodeDoc.XML")),
                    new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.CodeDoc.Cecil.XML")),
                    new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/TestLibrary1.XML"))
                );

                var cecilRepository = new CecilMemberRepository();
                var msdnRepository = new MsdnCodeDocMemberRepository();
                var supportingRepository = new CodeDocMergedMemberRepository(cecilRepository, msdnRepository);

                return new CodeDocRepositories(targetRepository, supportingRepository);
            }).InSingletonScope();
            return kernel;
        }


        protected override void OnApplicationStarted() {
            base.OnApplicationStarted();
            AreaRegistration.RegisterAllAreas();
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

    }
}