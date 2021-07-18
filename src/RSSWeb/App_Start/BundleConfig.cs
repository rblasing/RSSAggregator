using System;
using System.Web.Optimization;


namespace RSSWeb
{
   public class BundleConfig
   {
      public static void RegisterBundles(BundleCollection bundles)
      {
         bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));
      }
   }
}
