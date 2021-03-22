﻿// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass()]
   public class ItemTest : TestBase
   {
      [TestMethod()]
      public void constructorTest()
      {
         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(testItemNode);

         Item i = new Item(xDoc.DocumentElement);
         Assert.AreEqual("Test description", i.description);
         Assert.AreEqual(testItemNode.Replace("\r", "").Replace("\n", ""), i.itemXml);
         Assert.AreEqual(new DateTime(2020, 10, 29, 12, 37, 4, DateTimeKind.Utc), i.pubDate);
         Assert.AreEqual("Test title", i.title);
         Assert.AreEqual("https://www.anywhere.com/news/some-article/", i.url);
      }


      [TestMethod()]
      public void removeAdsTest()
      {
         Item i = new Item();
         PrivateObject po = new PrivateObject(i);

         string s = "<tag><img src=\"http://site/img/jpg\" /></tag>";
         string newXml = (string)po.Invoke("removeAds", new object[] { s });
         Assert.AreEqual("<tag></tag>", newXml);

         s = @"<span class=""itemdesc"">Article text<div class=""feedflare""><br><br><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:yIl2AUoC8zA"" >
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=yIl2AUoC8zA"" border=""0""></a><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:7Q72WNTAKBA"" >
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=7Q72WNTAKBA"" border=""0""></a><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:V_sGLiPBpWU"">
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?i=z0WNWlQE3Ck:uBEGyY3gYaI:V_sGLiPBpWU"" border=""0""></a><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:qj6IDK7rITs"">
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=qj6IDK7rITs"" border=""0""></a><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:gIN9vFwOqvQ"">
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?i=z0WNWlQE3Ck:uBEGyY3gYaI:gIN9vFwOqvQ"" border=""0""></a><br><br></div></span>";

         newXml = (string)po.Invoke("removeAds", new object[] { s });
         Assert.AreEqual(@"<span class=""itemdesc"">Article text</span>", newXml);
      }
   }
}