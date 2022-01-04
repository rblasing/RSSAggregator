using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace RSSWeb
{
   public class MvcApplication : System.Web.HttpApplication
   {
      protected void Application_Start()
      {
         AreaRegistration.RegisterAllAreas();
         FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
         RouteConfig.RegisterRoutes(RouteTable.Routes);
         BundleConfig.RegisterBundles(BundleTable.Bundles);

         // this will prevent "Server cannot append header after HTTP headers have been sent"
         // exceptions when downloading files to the client. To compensate for the
         // programmatic suppression of this header, it should be added via a web.config
         // parameter: //configuration/system.webServer/httpProtocol/customerHeaders/add[@name='X-Frame-Options', @value='SAMEORIGIN']
         System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
      }


      protected void Application_Error(object sender, EventArgs e)
      {
         var lastError = Server.GetLastError();

         if (lastError != null)
         {
            log4net.LogManager.GetLogger("RSSWeb").Error(lastError.Message);
            log4net.LogManager.GetLogger("RSSWeb").Error(lastError.StackTrace);

            var baseError = lastError.GetBaseException();

            if (baseError != null)
            {
               log4net.LogManager.GetLogger("RSSWeb").Error(baseError.Message);
               log4net.LogManager.GetLogger("RSSWeb").Error(baseError.StackTrace);
            }
         }

         Server.ClearError();
      }


      /// <summary>
      /// This environment variable is guaranteed to be populated for Azure-
      /// hosted web applications (as of 2021.5.22).
      /// </summary>
      /// <returns></returns>
      public static bool IsAzureHosted()
      {
         return !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
      }
   }
}
