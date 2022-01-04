using System;
using System.Configuration;
using System.Data.SqlClient;

using RSS;


namespace ServiceTests
{
   public class ServiceTestBase
   {
      private static SqlConnection _dbConn;
      private static Dal _dal;

      private static readonly string IisExpressPath = ConfigurationManager.AppSettings["iisExpressPath"];
      protected static readonly string IisPort = ConfigurationManager.AppSettings["iisPort"];
      private static System.Diagnostics.Process _iisProcess;


      public static void CInitialize()
      {
         string connStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
         _dbConn = new SqlConnection(connStr);
         _dbConn.Open();
         _dal = new Dal(_dbConn);
         InitDb();
      }


      public static void CCleanup()
      {
         _dbConn?.Close();
      }


      public void TCleanup()
      {
         if (_iisProcess != null  &&  _iisProcess.HasExited == false)
            _iisProcess.Kill();
      }


      protected static string GetApplicationPath(string appName)
      {
         var solutionFolder = System.IO.Path.GetDirectoryName(
            System.IO.Path.GetDirectoryName(
               System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));

         return System.IO.Path.Combine(solutionFolder, appName);
      }


      protected static void StartIis(string appPath)
      {
         _iisProcess = new System.Diagnostics.Process();
         _iisProcess.StartInfo.FileName = IisExpressPath;
         _iisProcess.StartInfo.Arguments = string.Format("/path:\"{0}\" /port:{1}",
         GetApplicationPath(appPath), IisPort);

         _iisProcess.Start();
      }


      protected static void InitDb()
      {
         SqlCommand cmd = new SqlCommand("DELETE FROM rss_item") { Connection = _dbConn };
         cmd.ExecuteNonQuery();

         cmd = new SqlCommand("DELETE FROM rss_feed") { Connection = _dbConn };
         cmd.ExecuteNonQuery();

         _dal.InsertFeed("Test feed 1", "http://testurl1/", false);
         _dal.InsertFeed("Test feed 2", "http://testurl2/", false);
         _dal.InsertFeed("Test feed 3", "http://testurl3/", false);

         ItemRow i1 = new ItemRow()
         {
            Description = "Item 1 desc",
            ItemXml = "<rss><channel><item><title>Item 1 title</title><link>Item 1 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 1, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 1 title",
            Url = "http://item1url/"
         };

         ItemRow i2 = new ItemRow()
         {
            Description = "Item 2 desc",
            ItemXml = "<rss><channel><item><title>Item 2 title</title><link>Item 2 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 2, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 2 title",
            Url = "http://item2url/"
         };

         ItemRow i3 = new ItemRow()
         {
            Description = "Item 3 desc",
            ItemXml = "<rss><channel><item><title>Item 3 title</title><link>Item 3 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 3, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 3 title",
            Url = "http://item3url/"
         };

         ItemRow i4 = new ItemRow()
         {
            Description = "Item 4 desc",
            ItemXml = "<rss><channel><item><title>Item 4 title</title><link>Item 4 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 4, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 4 title",
            Url = "http://item4url/"
         };

         ItemRow i5 = new ItemRow()
         {
            Description = "Item 5 desc",
            ItemXml = "<rss><channel><item><title>Item 5 title</title><link>Item 5 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 5, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 5 title",
            Url = "http://item5url/"
         };

         ItemRow i6 = new ItemRow()
         {
            Description = "Item 6 desc",
            ItemXml = "<rss><channel><item><title>Item 6 title</title><link>Item 6 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 6, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 6 title",
            Url = "http://item6url/"
         };

         var feed = _dal.GetFeeds();

         _dal.InsertItem(feed[0].FeedId, i1);
         _dal.InsertItem(feed[0].FeedId, i2);

         _dal.InsertItem(feed[1].FeedId, i3);
         _dal.InsertItem(feed[1].FeedId, i4);

         _dal.InsertItem(feed[2].FeedId, i5);
         _dal.InsertItem(feed[2].FeedId, i6);
      }
   }
}
