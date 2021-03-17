// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Configuration;
using System.ServiceProcess;


[assembly: log4net.Config.XmlConfigurator(Watch = true)]


namespace RSSAggregatorSvc
{
   static class Program
   {
      private static bool shouldStop = false;


      static void Main()
      {
         string isWebJob = ConfigurationManager.AppSettings["isWebJob"];

         RSSAggregatorSvc svc = new RSSAggregatorSvc();
         ServiceBase[] servicesToRun = new ServiceBase[] { svc };

         // if the exe is run from a shell, or if the config indicates
         // it's deployed as an Azure WebJob, then start as a console app
         if (isWebJob == "true"  ||  Environment.UserInteractive)
         {
            Console.WriteLine("Press ^C to exit...");
            Console.CancelKeyPress += UserCancel;

            RSSAggregator app = new RSSAggregator();
            app.aggregate(ref shouldStop);
         }
         else
            ServiceBase.Run(servicesToRun);
      }


      /// <summary>
      /// The RSSAggregator instance will monitor the reference to <c>shouldStop</c>
      /// and end execution when it is true.
      /// </summary>
      private static void UserCancel(object sender, ConsoleCancelEventArgs e)
      {
         // cancel the break action so that the aggregator thread can exit cleanly
         e.Cancel = true;

         Console.WriteLine("Exiting...");
         shouldStop = true;
      }
   }
}
