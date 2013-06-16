using System;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
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

                private readonly ObjectCache _cache = MemoryCache.Default;

                public MemberGenerator(ReflectionCodeDocMemberRepository repository, CodeDocRepositorySearchContext searchContext)
                    :base(repository, searchContext) { }

                private Uri CreateUri(CRefIdentifier cRef, MemberInfo memberInfo){
                    if (memberInfo != null) {
                        var type = memberInfo as Type ?? memberInfo.DeclaringType;
                        var searchKeywords = memberInfo.Name;
                        if (type != null) {
                            if(type != memberInfo)
                                searchKeywords += " " + type.Name;
                            var namespaceName = type.Namespace;
                            var typeName = type.Name;
                            var classFileUri = new Uri(
                                String.Format(
                                    "https://github.com/jbevain/cecil/blob/master/{0}/{1}.cs",
                                    namespaceName,
                                    typeName),
                                UriKind.Absolute);
                            // TODO: if the class file is found, use that
                            try{
                                var request = WebRequest.Create(classFileUri);
                                request.Timeout = 5000;
                                var response = request.GetResponse() as HttpWebResponse;
                                if (response.StatusCode == HttpStatusCode.OK)
                                    return classFileUri;
                            }catch{
                                ; // exception monster ate all the exceptions
                            }
                        }
                        return new Uri(
                            String.Format(
                                "https://github.com/jbevain/cecil/search?q={0}+repo%3Ajbevain%2Fcecil+extension%3Acs&type=Code",
                                Uri.EscapeDataString(searchKeywords)),
                            UriKind.Absolute);
                    }
                    return base.GetUri(cRef, memberInfo);
                }

                protected override Uri GetUri(CRefIdentifier cRef, MemberInfo memberInfo) {
                    var cacheKey = String.Format("{0}_GetUri({1})", GetType().FullName, cRef);
                    var uri = _cache[cacheKey] as Uri;
                    if(uri == null){
                        uri = CreateUri(cRef, memberInfo);
                        if (uri != null)
                            _cache[cacheKey] = uri;
                    }
                    return uri;
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