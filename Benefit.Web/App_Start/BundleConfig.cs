using System.Web.Optimization;

namespace Benefit.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //todo: add conventional autocomplete script and css load
            bundles.Add(new ScriptBundle("~/bundles/master").Include(
                        "~/Scripts/jquery-2.2.3.min.js",
                        "~/Scripts/jquery.autocomplete.min.js",
                        "~/Scripts/jquery-ui.js",
                        "~/Scripts/bootstrap.min.js",
                        "~/Scripts/owl.carousel.min.js",
                        "~/Scripts/scripts.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                                   "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/css/font-awesome.min.css",
                      "~/Content/owl.carousel.css",
                      "~/Content/jquery-ui.css",
                      "~/Content/css/main.css",
                      "~/Content/css/media.css",
                      "~/Content/autocomplete.css"));
        }
    }
}
