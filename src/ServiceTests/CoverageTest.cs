using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace ServiceTests
{
   [TestClass]
   public class CoverageTest
   {
      [TestMethod]
      public void CodeCoverageTest()
      {
         List<string> errors = new List<string>();
         Assembly testAssembly = this.GetType().Assembly;

         Assembly targetAssembly = typeof(RSSWCFSvc.RssService).Assembly;
         errors.AddRange(UnitTestCoverage.UnitTestCoverage.VerifyCoverage(targetAssembly, testAssembly));

         targetAssembly = typeof(RSSWebAPISvc.WebApiConfig).Assembly;
         errors.AddRange(UnitTestCoverage.UnitTestCoverage.VerifyCoverage(targetAssembly, testAssembly));

         if (errors.Count > 0)
            Assert.Fail(string.Join("\r\n", errors));
      }
   }
}
