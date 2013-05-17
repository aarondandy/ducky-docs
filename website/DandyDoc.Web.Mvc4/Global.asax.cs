using System;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;
using DandyDoc.XmlDoc;
using Ninject;
using Ninject.Web.Common;

namespace DandyDoc.Web.Mvc4
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : NinjectHttpApplication
    {
        protected override IKernel CreateKernel() {
            var kernel = new StandardKernel();
            kernel.Bind<ICodeDocMemberRepository>().ToMethod(x =>
                new ReflectionCodeDocMemberRepository(
                    new ReflectionCRefLookup(
                        typeof(ReflectionCRefLookup).Assembly,
                        typeof(ICodeDocMemberRepository).Assembly,
                        Assembly.ReflectionOnlyLoadFrom(HostingEnvironment.MapPath("~/bin/TestLibrary1.dll"))
                    ),
                    new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.Core.XML")),
                    new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/DandyDoc.CodeDoc.XML")),
                    new XmlAssemblyDocument(HostingEnvironment.MapPath("~/bin/TestLibrary1.XML"))
                )).InSingletonScope();
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