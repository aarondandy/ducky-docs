using Nancy;

namespace DandyDoc.Web.Nancy
{
    public class DocsModule : NancyModule
    {

        private static readonly string[] ViewExtensions = new[] {".cshtml", ".html", ".md"};

        public DocsModule() : base("/") {
            Get["/Api?cRef={cRef}"] = p => {
                return "TODO: lookup " + p.cRef;
            };
            Get["/"] = _ => View["default"];
            Get["/{viewPath*}"] = p => View[p.viewPath];
        }

    }
}