using System.Web.Optimization;


namespace Portfolio
{
   public class BundleConfig
   {
      public static void RegisterBundles(BundleCollection bundles)
      {
         bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
         bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));
         bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));
      }
   }
}
