using System.Web.Optimization;

namespace NyuLaw.Plutus.Web.App_Start
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
      
            bundles.Add(new ScriptBundle("~/bundles/flat-ui").Include(
                "~/Content/flat-ui/js/application.js",
                "~/Content/flat-ui/js/bootstrap-tooltip.js",
                "~/Content/flat-ui/js/custom_checkbox_and_radio.js",
                "~/Content/flat-ui/js/custom_radio.js",
                "~/Content/flat-ui/js/html5shiv.js",
                "~/Content/flat-ui/js/icon-font-ie7.js",
                "~/Content/flat-ui/js/jquery.dropkick-1.0.0.js",
                "~/Content/flat-ui/js/jquery.placeholder.js",
                "~/Content/flat-ui/js/jquery.tagsinput.js",
                "~/Content/flat-ui/js/jquery-1.8.2.min.js",
                "~/Content/flat-ui/js/jquery-ui-1.10.0.custom.min.js",
                "~/Content/flat-ui/js/lte-ie7-24.js"));


            bundles.Add(new ScriptBundle("~/bundles/common").Include("~/Scripts/Common.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css",
                                                                 "~/Content/simplegrid.css"));


#if (!DEBUG)
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}