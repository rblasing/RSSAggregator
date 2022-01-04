using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace UtilTests
{
   [TestClass]
   public class WSBaseTest
   {
      [TestMethod]
      public void ShouldThrottleTest()
      {
         int retrySec;
         string message;

         bool result = Util.WSBase.ShouldThrottle("1", 5, out retrySec, out message);
         Assert.IsFalse(result);

         result = Util.WSBase.ShouldThrottle("1", 5, out retrySec, out message);
         Assert.IsTrue(result);
         Assert.IsTrue(retrySec < 5  &&  retrySec > 0);
         Assert.AreEqual("Throttling 1", message);

         // 2 seconds after initial call
         System.Threading.Thread.Sleep(2000);
         result = Util.WSBase.ShouldThrottle("1", 5, out retrySec, out message);
         Assert.IsTrue(result);

         // 4 seconds after initial call
         System.Threading.Thread.Sleep(2000);
         result = Util.WSBase.ShouldThrottle("1", 5, out retrySec, out message);
         Assert.IsTrue(result);

         // 6 seconds after initial call.  Key should have expired from the cache
         System.Threading.Thread.Sleep(2000);
         result = Util.WSBase.ShouldThrottle("1", 5, out retrySec, out message);
         Assert.IsFalse(result);
      }
   }
}
