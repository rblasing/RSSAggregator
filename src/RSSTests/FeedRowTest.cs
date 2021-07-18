using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class FeedRowTest
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


      [TestMethod("See tests for implementations of IDAL ItemExists() and InsertItem()")]
      public void SaveTest()
      {
      }
   }
}
