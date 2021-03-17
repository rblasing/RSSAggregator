// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class ItemRowTest : TestBase
   {
      [TestMethod]
      public void toXmlTest()
      {
         ItemRow ir = new ItemRow();
         ir.description = "Test desc";
         ir.feedId = 9090;
         ir.feedName = "Test feed";
         ir.insDate = new DateTime(2020, 11, 25, 20, 15, 0, DateTimeKind.Utc);
         ir.itemXml = testItemNode;
         ir.pubDate = new DateTime(2020, 11, 26, 21, 30, 0, DateTimeKind.Utc);
         ir.title = "Test title";
         ir.url = "http://testurl";

         string xml = ir.toXml(true);
         Assert.AreEqual("<item type='newitem'><url>http://testurl</url><title>Test title</title><pubDate>2020-11-26 21:30:00Z</pubDate><insDate>2020-11-25 20:15:00Z</insDate><desc>Test desc</desc><feedName>Test feed</feedName></item>", xml);

         xml = ir.toXml(false);
         Assert.AreEqual("<item type='olditem'><url>http://testurl</url><title>Test title</title><pubDate>2020-11-26 21:30:00Z</pubDate><insDate>2020-11-25 20:15:00Z</insDate><desc>Test desc</desc><feedName>Test feed</feedName></item>", xml);
      }
   }
}
