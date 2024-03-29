﻿using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace UtilTests
{
   [TestClass]
   public class UtilCoverageTest
   {
      [TestMethod]
      public void CoverageTest()
      {
         Assembly testAssembly = this.GetType().Assembly;
         Assembly targetAssembly = typeof(Util.Js).Assembly;

         var errors = UnitTestCoverage.UnitTestCoverage.VerifyCoverage(targetAssembly, testAssembly);

         if (errors.Count > 0)
            Assert.Fail(string.Join("\r\n", errors));
      }
   }
}
