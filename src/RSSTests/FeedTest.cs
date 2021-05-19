// Copyright 2020 Richard Blasingame. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class FeedTest
   {
      [TestMethod]
      public void ConstructorTest()
      {
         FeedRow f = new FeedRow(1099, "testTitle", "testUri", false, true);
         Assert.AreEqual(1099, f.FeedId);
         Assert.AreEqual(true, f.IsActive);
         Assert.AreEqual("testTitle", f.Title);
         Assert.AreEqual("testUri", f.Url);
      }


      [TestMethod("See RSSReader Read() test")]
      public void RefreshTest()
      {
      }


      [TestMethod("See tests for implementations of IDAL ItemExists() and InsertItem()")]
      public void SaveTest()
      {
      }
   }
}
