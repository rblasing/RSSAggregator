using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace SQLServerUDFTests
{
   [TestClass]
   public class CoverageTest
   {
      [TestMethod]
      public void SQLServerUDFCoverageTest()
      {
         Assembly testAssembly = this.GetType().Assembly;
         Assembly targetAssembly = typeof(SQLServerUDF.SQLServerUDF).Assembly;

         var errors = UnitTestCoverage.UnitTestCoverage.VerifyCoverage(targetAssembly, testAssembly);

         if (errors.Count > 0)
            Assert.Fail(string.Join("\r\n", errors));
      }
   }
}
