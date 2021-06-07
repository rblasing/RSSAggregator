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
      public void ReadTest()
      {
         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(TestChannelNode);

         Channel c = new Channel();
         c.Read(xDoc.DocumentElement);

         Assert.AreEqual("Channel title", c.Title);
         Assert.AreEqual("Channel description", c.Description);
         Assert.IsNotNull(c.Items);
         Assert.AreEqual(new DateTime(2020, 10, 28, 1, 38, 0, DateTimeKind.Utc), c.LastBuildDate);
         Assert.AreEqual(new DateTime(2020, 10, 30, 1, 38, 0, DateTimeKind.Utc), c.PubDate);
         Assert.AreEqual("http://sitename/newsFeed.rss", c.Url);
      }
   }
}
