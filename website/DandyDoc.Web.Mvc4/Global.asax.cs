using System;
using System.Collections.ObjectModel;
using System.IO;
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

        public class NavNode : Collection<NavNode>
        {
            public string Title { get; set; }
            public Uri Link { get; set; }
            public string Icon { get; set; }
        }

        public class CRefNavNode : NavNode
        {
            public CRefIdentifier CRef { get; set; }
        }

        public class CodeDocRepositories
        {
            public CodeDocRepositories(ICodeDocMemberRepository targetRepository, ICodeDocMemberRepository supportingRepository) {
                TargetRepository = targetRepository;
                SupportingRepository = supportingRepository;
                _navRoot = new Lazy<NavNode>(CreateApiNavTree, true);
            }

            private readonly Lazy<NavNode> _navRoot; 

            public ICodeDocMemberRepository TargetRepository { get; private set; }

            public ICodeDocMemberRepository SupportingRepository { get; private set; }

            public CodeDocRepositorySearchContext CreateSearchContext(CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
                return new CodeDocRepositorySearchContext(new[] { TargetRepository, SupportingRepository }, detailLevel);
            }

            public NavNode NavRoot { get { return _navRoot.Value; } }

            private NavNode CreateApiNavTree() {
                var root = new NavNode {
                    Title = "API",
                    Icon = "icon-th",
                    Link = new Uri("~/Docs/Api", UriKind.Relative)
                };
                foreach(var namespaceModel in TargetRepository.Namespaces){
                    var namespaceNode = new CRefNavNode {
                        Title = namespaceModel.Title,
                        Icon = "icon-gift",
                        Link = namespaceModel.Uri,
                        CRef = namespaceModel.CRef
                    };
                    root.Add(namespaceNode);
                    foreach(var typeCRef in namespaceModel.TypeCRefs){
                        var typeModel = CreateSearchContext(CodeDocMemberDetailLevel.Minimum).Search(typeCRef);
                        var typeNode = new CRefNavNode {
                            Title = typeModel.Title,
                            Icon = "icon-cog",
                            Link = typeModel.Uri,
                            CRef = typeModel.CRef
                        };
                        namespaceNode.Add(typeNode);
                    }
                }
                return root;
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
                            /*// TODO: if the class file is found, use that
                            try{
                                var request = WebRequest.Create(classFileUri);
                                request.Timeout = 5000;
                                var response = request.GetResponse() as HttpWebResponse;
                                if (response.StatusCode == HttpStatusCode.OK)
                                    return classFileUri;
                            }catch{
                                ; // exception monster ate all the exceptions
                            }*/
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

        private static Assembly ReflectionOnlyResolveEventHandler(object sender, ResolveEventArgs args) {
            var assemblyName = new AssemblyName(args.Name);
            var binPath = HostingEnvironment.MapPath(String.Format("~/bin/{0}.dll", assemblyName.Name));
            if (File.Exists(binPath))
                return Assembly.ReflectionOnlyLoadFrom(binPath);
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        protected override IKernel CreateKernel() {

            // this is needed so reflection only load can find the other assemblies
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyResolveEventHandler;

            var kernel = new StandardKernel();

            kernel.Bind<CodeDocRepositories>().ToMethod(x => {
                var targetRepository = new ReflectionCodeDocMemberRepository(
                    new ReflectionCRefLookup(
                        typeof(ReflectionCRefLookup).Assembly,
                        typeof(ICodeDocMemberRepository).Assembly,
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

                var cecilRepository = new CecilMemberRepository();
                var msdnRepository = new MsdnCodeDocMemberRepository();
                var supportingRepository = new CodeDocMergedMemberRepository(cecilRepository, msdnRepository);

                return new CodeDocRepositories(targetRepository, supportingRepository);
            }).InSingletonScope();

            kernel.Bind<NavNode>().ToMethod(x => {
                var apiNavRoot = x.Kernel.Get<CodeDocRepositories>().NavRoot;
                return new NavNode(){
                    new NavNode{
                        Title = "Getting Started",
                        Icon = "icon-play",
                        Link = new Uri("~/Docs", UriKind.Relative)
                    },
                    apiNavRoot
                };
            });

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