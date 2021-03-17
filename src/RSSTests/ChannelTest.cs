// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class ChannelTest : TestBase
   {
      [TestMethod]
      public void readTest()
      {
         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(testChannelNode);

         Channel c = new Channel();
         c.read(xDoc.DocumentElement);

         Assert.AreEqual("Channel title", c.title);
         Assert.AreEqual("Channel description", c.description);
         Assert.IsNotNull(c.item);
         Assert.AreEqual(new DateTime(2020, 10, 28, 1, 38, 0, DateTimeKind.Utc), c.lastBuildDate);
         Assert.AreEqual(new DateTime(2020, 10, 30, 1, 38, 0, DateTimeKind.Utc), c.pubDate);
         Assert.AreEqual("http://sitename/newsFeed.rss", c.url);
      }
   }
}
