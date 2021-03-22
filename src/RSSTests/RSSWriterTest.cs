// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class RSSWriterTest : TestBase
   {
      Item[] items = new Item[2];


      public RSSWriterTest()
      {
         items[0] = new Item();
         items[0].description = "Desc 1";
         items[0].itemXml = testItemNode;
         items[0].pubDate = new DateTime(2020, 10, 11, 9, 10, 0, DateTimeKind.Utc);
         items[0].title = "Title 1";
         items[0].url = "http://testurl1/";

         items[1] = new Item();
         items[1].description = "Desc 2";
         items[1].itemXml = testItemNode;
         items[1].pubDate = new DateTime(2020, 11, 12, 10, 11, 0, DateTimeKind.Utc);
         items[1].title = "Title 2";
         items[1].url = "http://testurl2/";
      }


      [TestMethod]
      public void constructorTest()
      {
         RSSWriter r = new RSSWriter("Consolidated News", items, "http://testuri/", "http://testuri/test.xslx");
         PrivateObject po = new PrivateObject(r);

         Assert.AreEqual("http://testuri/test.xslx", (string)po.GetField("xsltUri"));

         SyndicationFeed writerFeed = (SyndicationFeed)po.GetField("feed");
         Assert.IsTrue((DateTime.UtcNow - writerFeed.LastUpdatedTime).TotalMinutes < 1);
         Assert.AreEqual("Consolidated News", writerFeed.Title.Text);
         Assert.AreEqual("http://testuri/", writerFeed.BaseUri.AbsoluteUri);
         Assert.AreEqual("30", writerFeed.ElementExtensions[0].GetObject<string>());

         List < SyndicationItem> writerItems = (List<SyndicationItem>)po.GetField("items");
         Assert.AreEqual(2, writerItems.Count);

         Assert.AreEqual(items[0].url, writerItems[0].Id);
         Assert.AreEqual(items[0].url, writerItems[0].Links[0].Uri.AbsoluteUri);
         Assert.AreEqual(items[0].pubDate, writerItems[0].PublishDate);
         Assert.AreEqual(items[0].description, writerItems[0].Summary.Text);
         Assert.AreEqual(items[0].title, writerItems[0].Title.Text);

         Assert.AreEqual(items[1].url, writerItems[1].Id);
         Assert.AreEqual(items[1].url, writerItems[1].Links[0].Uri.AbsoluteUri);
         Assert.AreEqual(items[1].pubDate, writerItems[1].PublishDate);
         Assert.AreEqual(items[1].description, writerItems[1].Summary.Text);
         Assert.AreEqual(items[1].title, writerItems[1].Title.Text);
      }


      [TestMethod("Validated by constructor test")]
      public void addItemTest()
      {
      }


      [TestMethod]
      public void writeTest()
      {
         string tempFile = Path.GetTempPath() + Path.GetRandomFileName();

         try
         {
            RSSWriter r = new RSSWriter("Consolidated News", items, "http://testuri/", "http://testuri/test.xslx");
            r.write(tempFile);

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
