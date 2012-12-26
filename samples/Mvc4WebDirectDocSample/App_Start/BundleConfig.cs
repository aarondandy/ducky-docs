using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Mvc4WebDirectDocSample
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles){
			bundles.UseCdn = true;

			bundles.Add(new StyleBundle("~/css/bootstrap").Include(
				"~/Content/bootstrap-responsive.css",
				"~/Content/bootstrap.css"
				));
			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/bootstrap.js"
				));

			bundles.Add(new StyleBundle("~/css/docs").Include(
				"~/Content/docs.css"
				));
		}
	}
}