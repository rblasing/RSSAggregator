using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class RssWriterTest : TestBase
   {
      private readonly Item[] _items = new Item[2];


      public RssWriterTest()
      {
         _items[0] = new Item()
         {
            Description = "Desc 1",
            ItemXml = TestItemNode,
            PubDate = new DateTime(2020, 10, 11, 9, 10, 0, DateTimeKind.Utc),
            Title = "Title 1",
            Url = "http://testurl1/"
         };

         _items[1] = new Item()
         {
            Description = "Desc 2",
            ItemXml = TestItemNode,
            PubDate = new DateTime(2020, 11, 12, 10, 11, 0, DateTimeKind.Utc),
            Title = "Title 2",
            Url = "http://testurl2/"
         };
      }


      [TestMethod]
      public void ConstructorTest()
      {
         RssWriter r = new RssWriter("Consolidated News", _items, "http://testuri/", "http://testuri/test.xslx");
         PrivateObject po = new PrivateObject(r);

         Assert.AreEqual("http://testuri/test.xslx", (string)po.GetField("_xsltUri"));

         SyndicationFeed writerFeed = (SyndicationFeed)po.GetField("_feed");
         Assert.IsTrue((DateTime.UtcNow - writerFeed.LastUpdatedTime).TotalMinutes < 1);
         Assert.AreEqual("Consolidated News", writerFeed.Title.Text);
         Assert.AreEqual("http://testuri/", writerFeed.BaseUri.AbsoluteUri);
         Assert.AreEqual("30", writerFeed.ElementExtensions[0].GetObject<string>());

         List < SyndicationItem> writerItems = (List<SyndicationItem>)po.GetField("_items");
         Assert.AreEqual(2, writerItems.Count);

         Assert.AreEqual(_items[0].Url, writerItems[0].Id);
         Assert.AreEqual(_items[0].Url, writerItems[0].Links[0].Uri.AbsoluteUri);
         Assert.AreEqual(_items[0].PubDate, writerItems[0].PublishDate);
         Assert.AreEqual(_items[0].Description, writerItems[0].Summary.Text);
         Assert.AreEqual(_items[0].Title, writerItems[0].Title.Text);

         Assert.AreEqual(_items[1].Url, writerItems[1].Id);
         Assert.AreEqual(_items[1].Url, writerItems[1].Links[0].Uri.AbsoluteUri);
         Assert.AreEqual(_items[1].PubDate, writerItems[1].PublishDate);
         Assert.AreEqual(_items[1].Description, writerItems[1].Summary.Text);
         Assert.AreEqual(_items[1].Title, writerItems[1].Title.Text);
      }


      [TestMethod("Validated by Constructor test")]
      public void AddItemTest()
      {
      }


      [TestMethod]
      public void WriteTest()
      {
         string tempFile = Path.GetTempPath() + Path.GetRandomFileName();

         try
         {
            RssWriter r = new RssWriter("Consolidated News", _items, "http://testuri/", "http://testuri/test.xslx");
            r.Write(tempFile);

            Assert.IsTrue(File.Exists(tempFile));
            FileInfo fi = new FileInfo(tempFile);
            Assert.IsTrue(fi.Length > 0);
         }
         finally
         {
            if (File.Exists(tempFile))
               File.Delete(tempFile);
         }
      }
   }
}
