using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace RSSTests
{
   [TestClass]
   public class RSSCoverageTest
   {
      [TestMethod]
      public void CoverageTest()
      {
         Assembly testAssembly = this.GetType().Assembly;
         Assembly targetAssembly = typeof(RSS.Channel).Assembly;

         var errors = UnitTestCoverage.UnitTestCoverage.VerifyCoverage(targetAssembly, testAssembly);

         if (errors.Count > 0)
            Assert.Fail(string.Join("\r\n", errors));
      }
   }
}
