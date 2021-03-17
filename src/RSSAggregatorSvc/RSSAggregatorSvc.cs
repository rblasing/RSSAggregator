// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.ServiceProcess;


namespace RSSAggregatorSvc
{
   public partial class RSSAggregatorSvc : ServiceBase
   {
      System.Threading.Thread workerThread = null;
      private bool stopRequested = false;


      public RSSAggregatorSvc()
      {
         InitializeComponent();
      }


      protected override void OnStart(string[] args)
      {
         System.Threading.ThreadStart start = new System.Threading.ThreadStart(ref worker);
         workerThread = new System.Threading.Thread(start);
         workerThread.Start();
      }


      /*public void ConsoleStart(string[] args)
      {
         OnStart(args);
      }*/


      protected override void OnStop()
      {
         // raise a flag that gives the worker thread an opportunity to
         // gracefully shutdown
         stopRequested = true;
      }


      private void worker()
      {
         RSSAggregator a = new RSSAggregator();
         a.aggregate(ref stopRequested);
      }
   }
}
