using System.Web.Optimization;

namespace DandyDocSite
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles) {
			bundles.UseCdn = true;

			bundles.Add(new StyleBundle("~/css/bootstrap").Include(
				"~/Content/bootstrap.css",
				"~/Content/bootstrap-responsive.css"
				));
			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/bootstrap.js"
				));

			bundles.Add(new StyleBundle("~/css/site").Include(
				"~/Content/site.css"
				));

			bundles.Add(new ScriptBundle("~/bundles/jquery", "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.8.2.min.js")
				.Include("~/Scripts/jquery-{version}.js"));
			bundles.Add(new ScriptBundle("~/bundles/jquery.color").Include("~/Scripts/jquery.color-2.1.0.js"));

			bundles.Add(new ScriptBundle("~/bundles/sh").Include(
				"~/Scripts/sh/shCore.js",
				"~/Scripts/sh/shBrushCSharp.js",
				"~/Scripts/sh/shBrushCss.js",
				"~/Scripts/sh/shBrushDiff.js",
				"~/Scripts/sh/shBrushJScript.js",
				"~/Scripts/sh/shBrushPlain.js",
				"~/Scripts/sh/shBrushPowerShell.js",
				"~/Scripts/sh/shBrushSql.js",
				"~/Scripts/sh/shBrushXml.js"
			));
			bundles.Add(new StyleBundle("~/css/sh").Include(
				"~/Content/sh/shCore.css",
				"~/Content/sh/shThemeVS2012Light.css",
				"~/Content/sh/scrollbarHack.css"
			));

			bundles.Add(new ScriptBundle("~/bundles/d3-scripts").Include(
				"~/Scripts/d3/d3.v2.js"
			));

		}
	}
}