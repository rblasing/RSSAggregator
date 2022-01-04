using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using RSS;
using Util;


namespace RssAggregatorSvc
{
   /// <summary>
   /// This service round-robins through a list of feeds, periodically
   /// retrieving the latest edition of each and persisting any new items to
   /// the database.
   /// 
   /// It also publishes the latest N items to a separate feed (N is configured
   /// via app.config).
   /// </summary>
   class RssAggregator
   {
      private static readonly string PubDir = ConfigurationManager.AppSettings["rssPublishDir"];
      private static readonly string PubUri = ConfigurationManager.AppSettings["rssPublishUri"];
      private static readonly string XsltUri = ConfigurationManager.AppSettings["rssXsltUri"];
      private static readonly string ConnStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
      private static readonly int SleepSecs = int.Parse((ConfigurationManager.AppSettings["sleepSec"]));
      private static readonly int PublishCount = int.Parse((ConfigurationManager.AppSettings["publishCount"]));

      private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("RSSAggregator");

      private static SqlConnection _dbConn;
      private static DalQuery _dal;
      private static FeedRow[] _feeds;


      public void Aggregate(ref bool stopRequested)
      {
         int feedIdx = 0;
         bool commonUpdateExecuted = false;

         try
         {
            while (true)
            {
               if (stopRequested)
                  return;

               if (_dbConn == null)
               {
                  _dbConn = new SqlConnection(ConnStr);
                  _dal = new DalQuery(_dbConn);
               }

               if (_dbConn.State != System.Data.ConnectionState.Open)
                  _dbConn.Open();

               if (_feeds == null)
               {
                  _feeds = _dal.GetFeeds().ToArray();
                  feedIdx = 0;
               }

               Logger?.Debug($"Current feed: {feedIdx} : {_feeds[feedIdx].Title}");

               // download a fresh copy of the next feed
               if (RefreshFeed(feedIdx))
                  PublishTopItems();

               // update the content of the common_word table at midnight
               if (DateTime.Now.Hour == 0  &&  !commonUpdateExecuted)
               {
                  _dal.UpdateCommonWords();
                  commonUpdateExecuted = true;
               }

               // one hour should allow enough time for the update to run, so
               // reset the executed flag at 1AM
               if (DateTime.Now.Hour == 1)
                  commonUpdateExecuted = false;

               _dbConn.Close();

               // loop back around
               if (++feedIdx >= _feeds.Length)
                  feedIdx = 0;

               System.Threading.Thread.Sleep(SleepSecs * 1000);
            }
         }
         catch (Exception e)
         {
            Logger?.Error($"Unhandled exception: {e.Message} : {e.StackTrace}");
         }
         finally
         {
            if (_dbConn != null)
            {
               if (_dbConn.State != System.Data.ConnectionState.Closed)
                  _dbConn.Close();

               _dbConn.Dispose();
            }
         }
      }


      /// <summary>
      /// Download the specified feed and save any new items to the DB.
      /// </summary>
      private static bool RefreshFeed(int feedIdx)
      {
         try
         {
            if (_feeds[feedIdx].IsActive)
            {
               _feeds[feedIdx].Refresh(_feeds[feedIdx].Url);
               _feeds[feedIdx].Save(_dal);

               var errors = _feeds[feedIdx].Errors();

               if (errors != null)
               {
                  foreach (ParserError e in errors)
                  {
                     string val = e.ElementValue;

                     try
                     {
                        val = val.FormattedXml();
                     }
                     catch (Exception)
                     {
                        // no worries if pretty printing fails
                     }

                     if (!e.Message.StartsWith("Possible ad"))
                        Logger?.Warn($"{_feeds[feedIdx].Title} : {e.Message} : {e.ElementName} : {val}");
                  }
               }
            }
         }
         catch (Exception e)
         {
            Logger?.Error($"Unhandled exception in readFeed(): {e.Message} : {e.StackTrace}");

            return false;
         }

         return true;
      }


      /// <summary>
      /// Publish a rollup of the latest retrieved items to a new feed.
      /// </summary>
      private static void PublishTopItems()
      {
         var itemsToPublish = _dal.GetTopItems(PublishCount);

         if (itemsToPublish == null)
            return;

         RssWriter rw = new RssWriter("Consolidated News", itemsToPublish.ToArray(),
            PubUri, XsltUri);

         rw.Write(PubDir);
      }
   }
}
