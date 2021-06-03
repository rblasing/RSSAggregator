using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using Util;


namespace UtilTests
{
   [TestClass]
   public class JsTest
   {
      [TestMethod]
      public void DateFromJsTicksTest()
      {
         Assert.AreEqual(new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Js.DateFromJsTicks(1622505600000));
      }

      
      [TestMethod]
      public void JsTicksFromDateTest()
      {
         Assert.AreEqual(1622505600000,
            Js.JsTicksFromDate(new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc)));
      }
   }
}
