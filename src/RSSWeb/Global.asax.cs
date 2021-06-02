// Copyright 2020 Richard Blasingame.All rights reserved.

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
      }


      protected void Application_Error(object sender, EventArgs e)
      {
         var lastError = Server.GetLastError();

         if (lastError != null)
         {
            log4net.LogManager.GetLogger("GlobalError").Error(lastError.Message);
            log4net.LogManager.GetLogger("GlobalError").Error(lastError.StackTrace);

            var baseError = lastError.GetBaseException();

            if (baseError != null)
            {
               log4net.LogManager.GetLogger("GlobalError").Error(baseError.Message);
               log4net.LogManager.GetLogger("GlobalError").Error(baseError.StackTrace);
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
