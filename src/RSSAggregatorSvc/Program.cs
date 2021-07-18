using System;
using System.Configuration;
using System.ServiceProcess;


[assembly: log4net.Config.XmlConfigurator(Watch = true)]


namespace RssAggregatorSvc
{
   static class Program
   {
      private static bool _shouldStop;


      static void Main()
      {
         string isWebJob = ConfigurationManager.AppSettings["isWebJob"];

         RssAggregatorSvc svc = new RssAggregatorSvc();
         ServiceBase[] servicesToRun = new ServiceBase[] { svc };

         // if the exe is run from a shell, or if the config indicates
         // it's deployed as an Azure WebJob, then start as a console app
         if (isWebJob == "true"  ||  Environment.UserInteractive)
         {
            Console.WriteLine("Press ^C to exit...");
            Console.CancelKeyPress += UserCancel;

            RssAggregator app = new RssAggregator();
            app.Aggregate(ref _shouldStop);
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
         _shouldStop = true;
      }
   }
}
