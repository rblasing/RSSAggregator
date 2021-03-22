// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class FeedTest
   {
      [TestMethod]
      public void constructorTest()
      {
         FeedRow f = new FeedRow(1099, "testTitle", "testUri", false, true);
         Assert.AreEqual(1099, f.feedId);
         Assert.AreEqual(true, f.isActive);
         Assert.AreEqual("testTitle", f.title);
         Assert.AreEqual("testUri", f.url);
      }


      [TestMethod("See RSSReader read() test")]
      public void refreshTest()
      {
      }


      [TestMethod("See tests for implementations of IDAL itemExists() and insertItem()")]
      public void saveTest()
      {
      }
   }
}
