// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Configuration;
using System.Data.SqlClient;

using RSS;
using Util;


namespace RSSAggregatorSvc
{
   /// <summary>
   /// This service round-robins through a list of feeds, periodically
   /// retrieving the latest edition of each and persisting any new items to
   /// the database.
   /// 
   /// It also publishes the latest N items to a separate feed (N is configured
   /// via app.config).
   /// </summary>
   class RSSAggregator
   {
      private static readonly string pubDir = ConfigurationManager.AppSettings["rssPublishDir"];
      private static readonly string pubUri = ConfigurationManager.AppSettings["rssPublishUri"];
      private static readonly string xsltUri = ConfigurationManager.AppSettings["rssXsltUri"];
      private static readonly string connStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
      private static readonly int sleepSecs = int.Parse((ConfigurationManager.AppSettings["sleepSec"]));
      private static readonly int publishCount = int.Parse((ConfigurationManager.AppSettings["publishCount"]));

      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("RSSAggregator");

      private static SqlConnection dbConn = null;
      private static DAL dal = null;
      private static FeedRow[] feeds = null;


      public void aggregate(ref bool stopRequested)
      {
         int feedIdx = 0;

         try
         {
            while (true)
            {
               if (stopRequested)
                  return;

               if (dbConn == null)
               {
                  dbConn = new SqlConnection(connStr);
                  dal = new DAL(dbConn);
               }

               if (dbConn.State != System.Data.ConnectionState.Open)
                  dbConn.Open();

               if (feeds == null)
               {
                  feeds = dal.getFeeds();
                  feedIdx = 0;
               }

               logger?.Debug($"Current feed: {feedIdx} : {feeds[feedIdx].title}");

               // download a fresh copy of the next feed
               if (RefreshFeed(feedIdx))
                  PublishTopItems();

               dbConn.Close();

               // loop back around
               if (++feedIdx >= feeds.Length)
                  feedIdx = 0;

               System.Threading.Thread.Sleep(sleepSecs * 1000);
            }
         }
         catch (Exception e)
         {
            logger?.Error($"Unhandled exception: {e.Message} : {e.StackTrace}");
         }
         finally
         {
            if (dbConn != null)
            {
               if (dbConn.State != System.Data.ConnectionState.Closed)
                  dbConn.Close();

               dbConn.Dispose();
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
            if (feeds[feedIdx].isActive)
            {
               feeds[feedIdx].refresh(feeds[feedIdx].url);
               feeds[feedIdx].save(dal);

               var errors = feeds[feedIdx].errors();

               if (errors != null)
               {
                  foreach (ParserError e in errors)
                  {
                     string val = e.elementValue;

                     try
                     {
                        val = val.FormattedXml();
                     }
                     catch (Exception)
                     {
                     }

                     if (!e.message.StartsWith("Possible ad"))
                        logger?.Warn($"{e.message} : {e.elementName} : {val}");
                  }
               }
            }
         }
         catch (Exception e)
         {
            logger?.Error($"Unhandled exception in readFeed(): {e.Message} : {e.StackTrace}");

            return false;
         }

         return true;
      }


      /// <summary>
      /// Publish a rollup of the latest retrieved items to a new feed.
      /// </summary>
      private static void PublishTopItems()
      {
         ItemRow[] itemsToPublish = dal.getTopItems(publishCount);

         if (itemsToPublish == null)
            return;

         RSSWriter fw = new RSSWriter("Consolidated News", itemsToPublish, pubUri, xsltUri);
         fw.write(pubDir);
      }
   }
}
