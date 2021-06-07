// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class RssBaseTest : TestBase
   {
      private readonly XmlDocument _xDoc = new XmlDocument();


      public RssBaseTest()
      {
         _xDoc.LoadXml(TestItemNode);
      }



      [TestMethod]
      public void SelectStringTest()
      {
         PrivateType pt = new PrivateType(typeof(RssBase));

         Assert.AreEqual("Test title", pt.InvokeStatic("SelectString", new object[] { _xDoc.DocumentElement, "title", true } ));
         Assert.AreEqual("Test description", pt.InvokeStatic("SelectString", new object[] { _xDoc.DocumentElement, "description", true } ));

         try
         {
            pt.InvokeStatic("SelectString", new object[] { _xDoc.DocumentElement, "nothing", true });
            Assert.Fail("RSSBase.SelectString(node, 'nothing', true) should have thrown RSSException");
         }
         catch (RssException)
         {
         }
         catch (Exception)
         {
            Assert.Fail("RSSBase.SelectString(node, 'nothing', true) should have thrown RSSException");
         }
      }


      [TestMethod]
      public void SelectIntTest()
      {
         PrivateType pt = new PrivateType(typeof(RssBase));
         Assert.AreEqual(30, pt.InvokeStatic("SelectInt", new object[] { _xDoc.DocumentElement, "ttl" }));
      }


      [TestMethod]
      public void SelectDecimalTest()
      {
         PrivateType pt = new PrivateType(typeof(RssBase));
         Assert.AreEqual((decimal)3.14, pt.InvokeStatic("SelectDecimal", new object[] { _xDoc.DocumentElement, "pi" }));
      }


      [TestMethod]
      public void SelectDateTimeTest()
      {
         PrivateType pt = new PrivateType(typeof(RssBase));

         Assert.AreEqual(
            new DateTime(2020, 10, 29, 12, 37, 4, DateTimeKind.Utc),
            pt.InvokeStatic("SelectDateTime", new object[] { _xDoc.DocumentElement, "pubDate" }));
      }
   }
}
