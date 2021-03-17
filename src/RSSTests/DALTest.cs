// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class DALTest
   {
      private string connStr;
      private SqlConnection dbConn = null;
      private DAL dal = null;


      public DALTest()
      {
         connStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
         dbConn = new SqlConnection(connStr);
         dbConn.Open();
         dal = new DAL(dbConn);
      }


      private void initDB()
      {
         SqlCommand cmd = new SqlCommand("DELETE FROM rss_item");
         cmd.Connection = dbConn;
         cmd.ExecuteNonQuery();

         cmd = new SqlCommand("DELETE FROM rss_feed");
         cmd.Connection = dbConn;
         cmd.ExecuteNonQuery();

         dal.insertFeed("Test feed 1", "http://testurl1/", false);
         dal.insertFeed("Test feed 2", "http://testurl2/", false);
         dal.insertFeed("Test feed 3", "http://testurl3/", false);

         Item i1 = new Item();
         i1.description = "Item 1 desc";
         i1.itemXml = "<rss><channel><item><title>Item 1 title</title><link>Item 1 link</link></item></channel></rss>";
         i1.pubDate = new DateTime(2020, 10, 1, 8, 20, 0, DateTimeKind.Utc);
         i1.title = "Item 1 title";
         i1.url = "http://item1url/";

         Item i2 = new Item();
         i2.description = "Item 2 desc";
         i2.itemXml = "<rss><channel><item><title>Item 2 title</title><link>Item 2 link</link></item></channel></rss>";
         i2.pubDate = new DateTime(2020, 10, 2, 8, 20, 0, DateTimeKind.Utc);
         i2.title = "Item 2 title";
         i2.url = "http://item2url/";

         Item i3 = new Item();
         i3.description = "Item 3 desc";
         i3.itemXml = "<rss><channel><item><title>Item 3 title</title><link>Item 3 link</link></item></channel></rss>";
         i3.pubDate = new DateTime(2020, 10, 3, 8, 20, 0, DateTimeKind.Utc);
         i3.title = "Item 3 title";
         i3.url = "http://item3url/";

         Item i4 = new Item();
         i4.description = "Item 4 desc";
         i4.itemXml = "<rss><channel><item><title>Item 4 title</title><link>Item 4 link</link></item></channel></rss>";
         i4.pubDate = new DateTime(2020, 10, 4, 8, 20, 0, DateTimeKind.Utc);
         i4.title = "Item 4 title";
         i4.url = "http://item4url/";

         Item i5 = new Item();
         i5.description = "Item 5 desc";
         i5.itemXml = "<rss><channel><item><title>Item 5 title</title><link>Item 5 link</link></item></channel></rss>";
         i5.pubDate = new DateTime(2020, 10, 5, 8, 20, 0, DateTimeKind.Utc);
         i5.title = "Item 5 title";
         i5.url = "http://item5url/";

         Item i6 = new Item();
         i6.description = "Item 6 desc";
         i6.itemXml = "<rss><channel><item><title>Item 6 title</title><link>Item 6 link</link></item></channel></rss>";
         i6.pubDate = new DateTime(2020, 10, 6, 8, 20, 0, DateTimeKind.Utc);
         i6.title = "Item 6 title";
         i6.url = "http://item6url/";

         FeedRow[] feed = dal.getFeeds();

         dal.insertItem(feed[0].feedId, i1);
         dal.insertItem(feed[0].feedId, i2);

         dal.insertItem(feed[1].feedId, i3);
         dal.insertItem(feed[1].feedId, i4);

         dal.insertItem(feed[2].feedId, i5);
         dal.insertItem(feed[2].feedId, i6);
      }


      [TestMethod]
      public void constructorTest()
      {
         PrivateObject po = new PrivateObject(dal);
         Assert.AreEqual(dbConn.ConnectionString, ((SqlConnection)po.GetField("dbConn")).ConnectionString);
      }


      [TestMethod]
      public void insertFeedTest()
      {
         initDB();
         Assert.IsTrue(dal.insertFeed("Test feed four", "http://testfeed/feed.rss", false));
         FeedRow[] feeds = dal.getFeeds();
         Assert.AreEqual(4, feeds.Length);

         Assert.AreEqual("Test feed four", feeds[3].title);
         Assert.AreEqual("http://testfeed/feed.rss", feeds[3].url);
         Assert.AreEqual(true, feeds[3].isActive);
      }


      [TestMethod]
      public void deleteFeedTest()
      {
         initDB();
         FeedRow[] feeds = dal.getFeeds();
         Assert.IsTrue(dal.deleteFeed(feeds[0].feedId));
         Assert.AreEqual(2, dal.getFeeds().Length);
      }


      [TestMethod]
      public void editFeedTest()
      {
         initDB();
         FeedRow[] feeds = dal.getFeeds();
         Assert.AreEqual(true, dal.editFeed(feeds[0].feedId, "New title", "http://editedurl/feed1.rss", false, false));

         feeds = dal.getFeeds();
         Assert.AreEqual(false, feeds[0].isActive);
         Assert.AreEqual("New title", feeds[0].title);
         Assert.AreEqual("http://editedurl/feed1.rss", feeds[0].url);
      }


      [TestMethod]
      public void getFeedsTest()
      {
         initDB();
         FeedRow[] feeds = dal.getFeeds();

         Assert.AreEqual(true, feeds[0].isActive);
         Assert.AreEqual("Test feed 1", feeds[0].title);
         Assert.AreEqual("http://testurl1/", feeds[0].url);

         Assert.AreEqual(true, feeds[1].isActive);
         Assert.AreEqual("Test feed 2", feeds[1].title);
         Assert.AreEqual("http://testurl2/", feeds[1].url);

         Assert.AreEqual(true, feeds[2].isActive);
         Assert.AreEqual("Test feed 3", feeds[2].title);
         Assert.AreEqual("http://testurl3/", feeds[2].url);
      }


      [TestMethod]
      public void insertItemTest()
      {
         initDB();

         Item i = new Item();
         i.description = "New item desc";
         i.itemXml = "<item><title>New item title</title></item>";
         i.pubDate = DateTime.UtcNow;
         i.title = "New item title";
         i.url = "http://newitemurl/feed.xml";

         FeedRow[] feeds = dal.getFeeds();
         Assert.AreEqual(true, dal.insertItem(feeds[0].feedId, i));

         ItemRow[] items = dal.getTopItems(1);
         Assert.AreEqual(i.description, items[0].description);
         Assert.AreEqual(i.itemXml, items[0].itemXml);
         Assert.AreEqual(i.pubDate, items[0].pubDate);
         Assert.AreEqual(i.title, items[0].title);
         Assert.AreEqual(i.url, items[0].url);
      }


      [TestMethod("Validated by insertItemTest")]
      public void getTopItemsTest()
      {
      }


      [TestMethod]
      public void getNewItemsTest1()
      {
         initDB();
         ItemRow[] items = dal.getNewItems(DateTime.UtcNow.AddHours(-1));
         Assert.AreEqual(6, items.Length);
      }


      [TestMethod]
      public void getNewItemsTest2()
      {
         initDB();
         ItemRow[] items = dal.getNewItems(DateTime.UtcNow.AddHours(-1), 3);
         Assert.AreEqual(3, items.Length);
      }


      [TestMethod]
      public void getMaxInsertDateTest()
      {
         initDB();
         DateTime maxDate = dal.getMaxInsertDate();

         SqlCommand cmd = new SqlCommand("SELECT MAX(ins_date) FROM rss_item", dbConn);
         DateTime targetDate = (DateTime)cmd.ExecuteScalar();
         Assert.AreEqual(targetDate, maxDate);
      }


      [TestMethod]
      public void itemExistsTest()
      {
         initDB();
         bool exists = dal.itemExists("nothing");
         Assert.IsFalse(exists);

         exists = dal.itemExists("http://item1url/");
         Assert.IsTrue(exists);
      }


      [TestMethod]
      public void getItemsByKeywordsTest()
      {
         initDB();
         ItemRow[] items = dal.getItemsByKeywords("Item");
         Assert.AreEqual(6, items.Length);

         items = dal.getItemsByKeywords("3");
         Assert.AreEqual(1, items.Length);

         items = dal.getItemsByKeywords("nothing");
         Assert.AreEqual(null, items);
      }


      [TestMethod]
      public void getItemsByRangeTest()
      {
         initDB();
         ItemRow[] items = dal.getItemsByRange(
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc));

         Assert.AreEqual(6, items.Length);

         items = dal.getItemsByRange(
            new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2101, 1, 1, 0, 0, 0, DateTimeKind.Utc));

         Assert.AreEqual(null, items);
      }


      [TestMethod]
      public void selectTest()
      {
         initDB();

         DataTable dt = dal.select("SELECT title FROM rss_feed");
         Assert.AreEqual(1, dt.Columns.Count);
         Assert.AreEqual("title", dt.Columns[0].ColumnName);
         Assert.AreEqual(3, dt.Rows.Count);
         Assert.AreEqual("Test feed 1", dt.Rows[0][0]);
         Assert.AreEqual("Test feed 2", dt.Rows[1][0]);
         Assert.AreEqual("Test feed 3", dt.Rows[2][0]);
      }


      [TestMethod]
      public void selectByKeywordTest()
      {
         initDB();
         DataTable dt = dal.selectByKeyword("Item 5");
         Assert.AreEqual(1, dt.Rows.Count);
         Assert.AreEqual("Item 5 title", dt.Rows[0]["title"]);
      }


      [TestMethod("Validated bt selectTest")]
      public void selectToDataTableTest()
      {
      }


      [TestMethod]
      public void createParameterTest()
      {
         initDB();
         PrivateObject po = new PrivateObject(dal);

         SqlCommand cmd = new SqlCommand();
         DbParameter p = (DbParameter)po.Invoke("createParameter", new object[] { cmd, "@p1", DbType.String, "test value" });
         Assert.AreEqual("@p1", p.ParameterName);
         Assert.AreEqual("test value", p.Value);
         Assert.AreEqual(DbType.String, p.DbType);
      }
   }
}
