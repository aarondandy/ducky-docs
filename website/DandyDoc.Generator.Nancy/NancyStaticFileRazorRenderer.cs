using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Conventions;
using Nancy.Localization;
using Nancy.ViewEngines;
using Nancy.ViewEngines.Razor;

namespace DandyDoc.Generator.Nancy
{
    public class NancyStaticFileRazorRenderer
    {

        public NancyStaticFileRazorRenderer() {
            Configuration = new DefaultRazorConfiguration();
            ViewEngine = new RazorViewEngine(Configuration);
            RootPathProvider = new DefaultRootPathProvider();
            ViewLocationProvider = new FileSystemViewLocationProvider(RootPathProvider, new DefaultFileSystemReader());
            ViewLocator = new DefaultViewLocator(ViewLocationProvider, new[]{ViewEngine});
            ViewLocationContext = new ViewLocationContext {
                Context = new NancyContext()
            };
            ViewLocationConventions = new ViewLocationConventions(new Func<string, object, ViewLocationContext, string>[0]);
            ViewResolver = new DefaultViewResolver(ViewLocator, ViewLocationConventions);
            ViewCache = new DefaultViewCache();
            ResourceAssemblyProvider = new ResourceAssemblyProvider();
            TextResource = new ResourceBasedTextResource(ResourceAssemblyProvider);
            RenderContext = new DefaultRenderContext(ViewResolver, ViewCache, TextResource, ViewLocationContext);
        }

        public IRazorConfiguration Configuration { get; private set; }
        public RazorViewEngine ViewEngine { get; private set; }
        public IRootPathProvider RootPathProvider { get; private set; }
        public FileSystemViewLocationProvider ViewLocationProvider { get; private set; }
        public IViewLocator ViewLocator { get; private set; }
        public ViewLocationContext ViewLocationContext { get; private set; }
        public ViewLocationConventions ViewLocationConventions { get; private set; }
        public IViewResolver ViewResolver { get; private set; }
        public IViewCache ViewCache { get; private set; }
        public IResourceAssemblyProvider ResourceAssemblyProvider { get; private set; }
        public ITextResource TextResource { get; private set; }
        public DefaultRenderContext RenderContext { get; private set; }

        public Response CreateResponse(ViewLocationResult viewLocationResult, object model) {
            Contract.Requires(viewLocationResult != null);
            return ViewEngine.RenderView(viewLocationResult, model, RenderContext);
        }

    }
}
