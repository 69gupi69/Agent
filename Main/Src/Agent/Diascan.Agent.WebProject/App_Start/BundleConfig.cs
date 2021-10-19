using System.Web;
using System.Web.Optimization;

namespace Diascan.Agent.WebProject
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Scripts/base").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/kendo/kendo.all.js",
                "~/Scripts/kendo/jszip.min.js",
                "~/Scripts/kendo/cultures/kendo.culture.ru-RU.js",
                "~/Scripts/kendo/messages/kendo.messages.ru-RU.js",
                "~/Scripts/kendo.messages.dia.ru-RU.js"//,
                /*"~/Scripts/datefix.js"*/));
            bundles.Add(new StyleBundle("~/Content/bootstrap-css").Include("~/Content/bootstrap.min.css"));
            bundles.Add(new StyleBundle("~/Content/kendo/kendo-css").Include(
                "~/Content/kendo/kendo.common.min.css",
                "~/Content/kendo/kendo.common-bootstrap.min.css",
                "~/Content/kendo/kendo.bootstrap.min.css"
            ));
            bundles.Add(new StyleBundle("~/Content/site-css").Include("~/Content/Site.css"));
        }
    }
}
