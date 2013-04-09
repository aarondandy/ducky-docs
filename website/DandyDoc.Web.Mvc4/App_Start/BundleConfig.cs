using System.Web.Optimization;

namespace DandyDoc.Web.Mvc4
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles) {
            bundles.UseCdn = true;

            bundles.Add(new StyleBundle("~/bootstrap-css").Include(
                "~/Content/bootstrap/css/bootstrap.css",
                "~/Content/bootstrap/css/bootstrap-responsive.css"
                ));
            bundles.Add(new ScriptBundle("~/bootstrap-js").Include(
                "~/Content/bootstrap/js/bootstrap.js"
                ));

            bundles.Add(new ScriptBundle("~/jquery", "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.0.min.js")
                .Include("~/Content/jquery/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/jquery.color").Include("~/Content/jquery/jquery.color-2.1.0.js"));

            bundles.Add(new ScriptBundle("~/sh-js").Include(
                "~/Content/syntaxhighlighter/shCore.js",
                "~/Content/syntaxhighlighter/shBrushCSharp.js",
                "~/Content/syntaxhighlighter/shBrushCss.js",
                "~/Content/syntaxhighlighter/shBrushDiff.js",
                "~/Content/syntaxhighlighter/shBrushJScript.js",
                "~/Content/syntaxhighlighter/shBrushPlain.js",
                "~/Content/syntaxhighlighter/shBrushPowerShell.js",
                "~/Content/syntaxhighlighter/shBrushSql.js",
                "~/Content/syntaxhighlighter/shBrushXml.js"
            ));
            bundles.Add(new StyleBundle("~/sh-css").Include(
                "~/Content/syntaxhighlighter/shCore.css",
                "~/Content/syntaxhighlighter/shThemeVS2012Light.css",
                "~/Content/syntaxhighlighter/scrollbarHack.css"
            ));

            bundles.Add(new ScriptBundle("~/d3-js").Include(
                "~/Content/d3/d3.v2.js"
            ));

            bundles.Add(new StyleBundle("~/site-css").Include(
                "~/Content/site.css"
                ));
            bundles.Add(new ScriptBundle("~/site-js").Include(
                "~/Content/site.js"
            ));

        }
    }
}