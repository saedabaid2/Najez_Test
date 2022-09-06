using System.Web;
using System.Web.Optimization;

namespace Almanea
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/vendor/jquery/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/unobtrusiveajax").Include(
                 "~/Scripts/jquery.unobtrusive*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/vendor/bootstrap/js/bootstrap.bundle.min.js",
                      "~/vendor/ruang-admin.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/assets/css/material-dashboard.css",
                      //"~/Content/bootstrap.css",
                      "~/assets/demo/demo.css"));

            bundles.Add(new ScriptBundle("~/bundles/dropzone").Include(
                  "~/Scripts/dropzone/dropzone.js",
                  "~/Scripts/dropzone.js"));

            bundles.Add(new ScriptBundle("~/bundles/mvcfoolproof").Include(
                "~/Scripts/foolproof/mvcfoolproof.unobtrusive.min.js"));
        }
    }
}
