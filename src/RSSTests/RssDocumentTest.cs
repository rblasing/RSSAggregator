// Copyright 2020 Richard Blasingame. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class RssDocumentTest : TestBase
   {
      [TestMethod]
      public void ConstructorTest()
      {
         RssDocument r = new RssDocument(TestRssNode);
         Assert.AreEqual((decimal)2, r.Version);

         int itemCount = 0;

         foreach (var item in r.Channel.Items)
            itemCount++;

         Assert.AreEqual(1, itemCount);
      }
   }
}
