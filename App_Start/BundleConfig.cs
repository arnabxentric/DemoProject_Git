using System.Web;
using System.Web.Optimization;

namespace XenERP
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-2.1.0").Include(
                        "~/Scripts/jquery-2.1.0.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui-1.10.3").Include(
                        "~/Scripts/jquery-ui-1.10.3.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                       "~/js/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/apps").Include(
                      "~/js/AdminLTE/app.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));


            bundles.Add(new StyleBundle("~/css/bootstrap").Include("~/css/bootstrap.min.css"));
            bundles.Add(new StyleBundle("~/css/font-awesome").Include("~/css/font-awesome.min.css"));
            bundles.Add(new StyleBundle("~/css/ionicons").Include("~/css/ionicons.min.css"));
            bundles.Add(new StyleBundle("~/css/AdminLTE").Include("~/css/AdminLTE.css"));

        }
    }
}