using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class ItemRowTest : TestBase
   {
      [TestMethod]
      public void ToXmlTest()
      {
         ItemRow ir = new ItemRow()
         {
            Description = "Test desc",
            FeedId = 9090,
            FeedName = "Test feed",
            InsDate = new DateTime(2020, 11, 25, 20, 15, 0, DateTimeKind.Utc),
            ItemXml = TestItemNode,
            PubDate = new DateTime(2020, 11, 26, 21, 30, 0, DateTimeKind.Utc),
            Title = "Test title",
            Url = "http://testurl"
         };
         
         string xml = ir.ToXml(true);
         Assert.AreEqual("<item type='newitem'><url>http://testurl</url><title>Test title</title><pubDate>2020-11-26T21:30:00.0000000Z</pubDate><insDate>2020-11-25T20:15:00.0000000Z</insDate><desc>Test desc</desc><feedName>Test feed</feedName></item>", xml);

         xml = ir.ToXml(false);
         Assert.AreEqual("<item type='olditem'><url>http://testurl</url><title>Test title</title><pubDate>2020-11-26T21:30:00.0000000Z</pubDate><insDate>2020-11-25T20:15:00.0000000Z</insDate><desc>Test desc</desc><feedName>Test feed</feedName></item>", xml);
      }
   }
}
