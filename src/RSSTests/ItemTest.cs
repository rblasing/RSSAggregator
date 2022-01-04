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
      public void ConstructorTest()
      {
         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(TestItemNode);

         Item i = new Item(xDoc.DocumentElement);
         Assert.AreEqual("Test description", i.Description);
         Assert.AreEqual(TestItemNode.Replace("\r", "").Replace("\n", ""), i.ItemXml);
         Assert.AreEqual(new DateTime(2020, 10, 29, 12, 37, 4, DateTimeKind.Utc), i.PubDate);
         Assert.AreEqual("Test title", i.Title);
         Assert.AreEqual("https://www.anywhere.com/news/some-article/", i.Url);
      }


      [TestMethod()]
      [DataRow("Test string",                        "Test string")]
      [DataRow("<b>Test</b> string",                 "Test string")]
      [DataRow("Test &amp; string",                  "Test &amp; string")]
      [DataRow("Test & string",                      "Test & string")]
      [DataRow("Test <img src='image.jpg' />string", "Test string")]
      public void DescriptionInnerTextTest(string input, string innerText)
      {
         Item i = new Item { Description = input };
         Assert.AreEqual(innerText, i.DescriptionInnerText());

         /*Item i = new Item { Description = "Test string" };
         Assert.AreEqual("Test string", i.DescriptionInnerText());

         i.Description = "<b>Test</b> string";
         Assert.AreEqual("Test string", i.DescriptionInnerText());

         i.Description = "Test &amp; string";
         Assert.AreEqual("Test &amp; string", i.DescriptionInnerText());

         i.Description = "Test & string";
         Assert.AreEqual("Test & string", i.DescriptionInnerText());

         i.Description = "Test <img src='image.jpg' />string";
         Assert.AreEqual("Test string", i.DescriptionInnerText());*/
      }


      [TestMethod()]
      public void RemoveAdsTest()
      {
         PrivateType pt = new PrivateType(typeof(Item));

         string s = "<tag><img src=\"http://site/img/jpg\" /></tag>";
         string newXml = (string)pt.InvokeStatic("RemoveAds", new object[] { s });
         Assert.AreEqual("<tag></tag>", newXml);

         s = @"<span class=""itemdesc"">Article text<div class=""feedflare""><br><br><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:yIl2AUoC8zA"" >
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=yIl2AUoC8zA"" border=""0""></a><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:7Q72WNTAKBA"" >
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=7Q72WNTAKBA"" border=""0""></a><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:V_sGLiPBpWU"">
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?i=z0WNWlQE3Ck:uBEGyY3gYaI:V_sGLiPBpWU"" border=""0""></a><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:qj6IDK7rITs"">
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=qj6IDK7rITs"" border=""0""></a><a href = ""http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:gIN9vFwOqvQ"">
<img src=""http://feeds.feedburner.com/~ff/rss/cnn_topstories?i=z0WNWlQE3Ck:uBEGyY3gYaI:gIN9vFwOqvQ"" border=""0""></a><br><br></div></span>";

         newXml = (string)pt.InvokeStatic("RemoveAds", new object[] { s });
         Assert.AreEqual(@"<span class=""itemdesc"">Article text</span>", newXml);
      }


      [TestMethod()]
      public void EqualsTest()
      {
         Item i1 = new Item()
         {
            Description = "d",
            ItemXml = "<item />",
            PubDate = new DateTime(2020, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Title = "t",
            Url = "u"
         };

         Item i2 = new Item()
         {
            Description = "d",
            ItemXml = "<item />",
            PubDate = new DateTime(2020, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Title = "t",
            Url = "u"
         };

         Assert.AreEqual(i1, i2);

         i2.Description = "d2";
         Assert.AreNotEqual(i1, i2);
         i2.Description = i1.Description;

         i2.ItemXml = "<item2 />";
         Assert.AreNotEqual(i1, i2);
         i2.ItemXml = i1.ItemXml;

         i2.PubDate = new DateTime(2021, 6, 2, 0, 0, 0, DateTimeKind.Utc);
         Assert.AreNotEqual(i1, i2);
         i2.PubDate = i1.PubDate;

         i2.Title = "t2";
         Assert.AreNotEqual(i1, i2);
         i2.Title = i1.Title;

         i2.Url = "u2";
         Assert.AreNotEqual(i1, i2);
      }
   }
}