// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class RSSDocumentTest : TestBase
   {
      [TestMethod]
      public void constructorTest()
      {
         RSSDocument r = new RSSDocument(testRssNode);
         Assert.AreEqual((decimal)2, r.version);
         Assert.AreEqual(1, r.channel.item.Length);
      }
   }
}
