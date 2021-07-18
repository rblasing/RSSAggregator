using System;
using System.ServiceProcess;


namespace RssAggregatorSvc
{
   public partial class RssAggregatorSvc : ServiceBase
   {
      private System.Threading.Thread _workerThread;
      private bool _stopRequested;


      public RssAggregatorSvc() => InitializeComponent();


      protected override void OnStart(string[] args)
      {
         System.Threading.ThreadStart start = new System.Threading.ThreadStart(ref Worker);
         _workerThread = new System.Threading.Thread(start);
         _workerThread.Start();
      }


      // raise a flag that gives the worker thread an opportunity to
      // gracefully shutdown
      protected override void OnStop() => _stopRequested = true;


      private void Worker()
      {
         RssAggregator a = new RssAggregator();
         a.Aggregate(ref _stopRequested);
      }
   }
}
