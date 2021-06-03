// Copyright 2020 Richard Blasingame.All rights reserved.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Util;


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
