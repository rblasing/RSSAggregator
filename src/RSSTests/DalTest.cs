// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class DalTest
   {
      private readonly SqlConnection _dbConn;
      private readonly Dal _dal;


      public DalTest()
      {
         string connStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
         _dbConn = new SqlConnection(connStr);
         _dbConn.Open();
         _dal = new Dal(_dbConn);
      }


      private void InitDb()
      {
         SqlCommand cmd = new SqlCommand("DELETE FROM rss_item") { Connection = _dbConn };
         cmd.ExecuteNonQuery();

         cmd = new SqlCommand("DELETE FROM rss_feed") { Connection = _dbConn };
         cmd.ExecuteNonQuery();

         _dal.InsertFeed("Test feed 1", "http://testurl1/", false);
         _dal.InsertFeed("Test feed 2", "http://testurl2/", false);
         _dal.InsertFeed("Test feed 3", "http://testurl3/", false);

         Item i1 = new Item()
         {
            Description = "Item 1 desc North Carolina",
            ItemXml = "<item><title>Item 1 title</title><link>Item 1 link</link><pubDate>Mon, 24 May 2021 00:00:00 Z</pubDate></item>",
            PubDate = new DateTime(2020, 10, 1, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 1 title",
            Url = "http://item1url/"
         };

         Item i2 = new Item()
         {
            Description = "Item 2 desc N Carolina",
            ItemXml = "<item><title>Item 2 title</title><link>Item 2 link</link><pubDate>Tue, 25 May 2021 00:00:00 Z</pubDate></item>",
            PubDate = new DateTime(2020, 10, 2, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 2 title",
            Url = "http://item2url/"
         };

         Item i3 = new Item()
         {
            Description = "Item 3 desc",
            ItemXml = "<item><title>Item 3 title</title><link>Item 3 link</link><pubDate>Wed, 26 May 2021 00:00:00 Z</pubDate></item>",
            PubDate = new DateTime(2020, 10, 3, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 3 title",
            Url = "http://item3url/"
         };

         Item i4 = new Item()
         {
            Description = "Item 4 desc",
            ItemXml = "<item><title>Item 4 title</title><link>Item 4 link</link><pubDate>Thu, 27 May 2021 00:00:00 Z</pubDate></item>",
            PubDate = new DateTime(2020, 10, 4, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 4 title",
            Url = "http://item4url/"
         };

         Item i5 = new Item()
         {
            Description = "Item 5 desc",
            ItemXml = "<item><title>Item 5 title</title><link>Item 5 link</link><pubDate>Fri, 28 May 2021 00:00:00 Z</pubDate></item>",
            PubDate = new DateTime(2020, 10, 5, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 5 title",
            Url = "http://item5url/"
         };

         Item i6 = new Item()
         {
            Description = "Item 6 desc",
            ItemXml = "<item><title>Item 6 title</title><link>Item 6 link</link><pubDate>Sat, 29 May 2021 00:00:00 Z</pubDate></item>",
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


      [TestMethod]
      public void ConstructorTest()
      {
         PrivateObject po = new PrivateObject(_dal);
         Assert.AreEqual(_dbConn.ConnectionString, ((SqlConnection)po.GetField("_dbConn")).ConnectionString);
      }


      [TestMethod]
      public void InsertFeedTest()
      {
         InitDb();
         Assert.IsTrue(_dal.InsertFeed("Test feed four", "http://testfeed/feed.rss", false));
         var feeds = _dal.GetFeeds();
         Assert.AreEqual(4, feeds.Count);

         Assert.AreEqual("Test feed four", feeds[3].Title);
         Assert.AreEqual("http://testfeed/feed.rss", feeds[3].Url);
         Assert.AreEqual(true, feeds[3].IsActive);
      }


      [TestMethod]
      public void DeleteFeedTest()
      {
         InitDb();
         var feeds = _dal.GetFeeds();
         Assert.IsTrue(_dal.DeleteFeed(feeds[0].FeedId));
         Assert.AreEqual(2, _dal.GetFeeds().Count);
      }


      [TestMethod]
      public void EditFeedTest()
      {
         InitDb();
         var feeds = _dal.GetFeeds();
         Assert.AreEqual(true, _dal.EditFeed(feeds[0].FeedId, "New title", "http://editedurl/feed1.rss", false, false));

         feeds = _dal.GetFeeds();
         Assert.AreEqual(false, feeds[0].IsActive);
         Assert.AreEqual("New title", feeds[0].Title);
         Assert.AreEqual("http://editedurl/feed1.rss", feeds[0].Url);
      }


      [TestMethod]
      public void GetFeedsTest()
      {
         InitDb();
         var feeds = _dal.GetFeeds();

         Assert.AreEqual(true, feeds[0].IsActive);
         Assert.AreEqual("Test feed 1", feeds[0].Title);
         Assert.AreEqual("http://testurl1/", feeds[0].Url);

         Assert.AreEqual(true, feeds[1].IsActive);
         Assert.AreEqual("Test feed 2", feeds[1].Title);
         Assert.AreEqual("http://testurl2/", feeds[1].Url);

         Assert.AreEqual(true, feeds[2].IsActive);
         Assert.AreEqual("Test feed 3", feeds[2].Title);
         Assert.AreEqual("http://testurl3/", feeds[2].Url);
      }


      [TestMethod]
      public void InsertItemTest()
      {
         InitDb();

         Item i = new Item()
         {
            Description = "New item desc",
            ItemXml = "<item><title>New item title</title></item>",
            PubDate = DateTime.UtcNow,
            Title = "New item title",
            Url = "http://newitemurl/feed.xml"
         };

         var feeds = _dal.GetFeeds();
         Assert.AreEqual(true, _dal.InsertItem(feeds[0].FeedId, i));

         var items = _dal.GetTopItems(1);
         Assert.AreEqual(i.Description, items[0].Description);
         Assert.AreEqual(i.ItemXml, items[0].ItemXml);
         Assert.AreEqual(i.PubDate, items[0].PubDate);
         Assert.AreEqual(i.Title, items[0].Title);
         Assert.AreEqual(i.Url, items[0].Url);
      }


      [TestMethod("Validated by InsertItemTest")]
      public void GetTopItemsTest()
      {
      }


      [TestMethod]
      public void GetNewItemsTest()
      {
         InitDb();
         var items = _dal.GetNewItems(DateTime.UtcNow.AddHours(-1));
         Assert.AreEqual(6, items.Count);
      }


      [TestMethod]
      public void GetNewItemsTest2()
      {
         InitDb();
         var items = _dal.GetNewItems(DateTime.UtcNow.AddHours(-1), 3);
         Assert.AreEqual(3, items.Count);
      }


      [TestMethod]
      public void GetMaxInsertDateTest()
      {
         InitDb();
         DateTime maxDate = _dal.GetMaxInsertDate();

         SqlCommand cmd = new SqlCommand("SELECT MAX(ins_date) FROM rss_item", _dbConn);
         DateTime targetDate = (DateTime)cmd.ExecuteScalar();
         Assert.AreEqual(targetDate, maxDate);
      }


      [TestMethod]
      public void ItemExistsTest()
      {
         InitDb();
         bool exists = _dal.ItemExists("nothing");
         Assert.IsFalse(exists);

         exists = _dal.ItemExists("http://item1url/");
         Assert.IsTrue(exists);
      }


      [TestMethod]
      public void GetItemsByKeywordsTest()
      {
         InitDb();
         var items = _dal.GetItemsByKeywords("Item");
         Assert.AreEqual(6, items.Count);

         items = _dal.GetItemsByKeywords("3");
         Assert.AreEqual(1, items.Count);

         items = _dal.GetItemsByKeywords("nothing");
         Assert.AreEqual(null, items);
      }


      [TestMethod]
      public void GetItemsByRangeTest()
      {
         InitDb();

         var items = _dal.GetItemsByRange(
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc));

         Assert.AreEqual(6, items.Count);

         items = _dal.GetItemsByRange(
            new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2101, 1, 1, 0, 0, 0, DateTimeKind.Utc));

         Assert.AreEqual(null, items);
      }


      [TestMethod()]
      public void GetItemsByStateTest()
      {

         InitDb();
         var items = _dal.GetItemsByState("NC", 10);
         Assert.IsNotNull(items);
         Assert.AreEqual(2, items.Count);
      }


      [TestMethod()]
      public void GetStateActivityByRangeTest()
      {
         InitDb();
         var activity = _dal.GetStateActivityByRange(8);
         Assert.IsNotNull(activity);
         Assert.AreEqual(1, activity.Count);
         Assert.AreEqual(2, activity["NC"]);
      }


      [TestMethod]
      public void GetTrendingWordsTest()
      {
         InitDb();
         var words = _dal.GetTrendingWords(8);
         Assert.IsNotNull(words);
         Assert.AreEqual(5, words.Count);
      }


      [TestMethod]
      public void GetTrendingWordsLinqTest()
      {
         InitDb();
         PrivateObject po = new PrivateObject(_dal);

         var words = (Dictionary<string, int>)po.Invoke("GetTrendingWordsLinq", new object[] { 8000 });
         Assert.IsNotNull(words);
         Assert.AreEqual(5, words.Count);
      }


      [TestMethod]
      public void GetWordHistoryTest()
      {
         InitDb();
         var counts = _dal.GetWordHistory("title", 8);
         Assert.IsNotNull(counts);
         Assert.AreEqual(9, counts.Count);
      }


      [TestMethod]
      public void GetIgnorableWordsTest()
      {
         InitDb();
         var counts = _dal.GetIgnorableWords();
         Assert.IsNotNull(counts);
         Assert.IsTrue(counts.Count > 0);
      }


      [TestMethod]
      public void ReadToItemListTest()
      {
         InitDb();
         SqlCommand cmd = new SqlCommand(
            "SELECT i.feed_id, f.title, i.ins_date, i.pub_date, i.title, i.description, i.url, i.xml FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id",
            _dbConn);

         SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

         PrivateType pt = new PrivateType(typeof(Dal));
         var items = (IReadOnlyList<ItemRow>)pt.InvokeStatic("ReadToItemList", new object[] { dr });
         Assert.IsNotNull(items);
         Assert.AreEqual(6, items.Count);
         Assert.AreEqual("Item 1 desc North Carolina", items[0].Description);
         Assert.AreEqual("Test feed 1", items[0].FeedName);
         Assert.AreNotEqual(null, items[0].InsDate);
         Assert.AreEqual("<item><title>Item 1 title</title><link>Item 1 link</link><pubDate>Mon, 24 May 2021 00:00:00 Z</pubDate></item>", items[0].ItemXml);
         Assert.AreEqual(new DateTime(2020, 10, 1, 8, 20, 0, DateTimeKind.Utc), items[0].PubDate);
         Assert.AreEqual("Item 1 title", items[0].Title);
         Assert.AreEqual("http://item1url/", items[0].Url);

         dr.Close();
      }


      [TestMethod]
      public void SelectExecTest()
      {
         InitDb();

         DataTable dt = _dal.SelectExec("SelectDailyDistribution");
         Assert.AreEqual(2, dt.Columns.Count);
         Assert.AreEqual(6, dt.Rows.Count);
      }


      [TestMethod]
      public void SelectByKeywordTest()
      {
         InitDb();
         DataTable dt = _dal.SelectByKeyword("Item 5");
         Assert.AreEqual(1, dt.Rows.Count);
         Assert.AreEqual("Item 5 title", dt.Rows[0]["title"]);
      }


      [TestMethod("Validated by SelectExecTest")]
      public void SelectToDataTableTest()
      {
      }


      [TestMethod]
      public void CreateParameterTest()
      {
         InitDb();
         PrivateType pt = new PrivateType(typeof(Dal));

         SqlCommand cmd = new SqlCommand();
         DbParameter p = (DbParameter)pt.InvokeStatic("CreateParameter", new object[] { cmd, "@p1", DbType.String, "test value" });
         Assert.AreEqual("@p1", p.ParameterName);
         Assert.AreEqual("test value", p.Value);
         Assert.AreEqual(DbType.String, p.DbType);
      }
   }
}
