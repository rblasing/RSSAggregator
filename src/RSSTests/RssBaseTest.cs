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
         RssBase r = new RssBase();
         PrivateObject po = new PrivateObject(r);

         Assert.AreEqual("Test title", po.Invoke("SelectString", new object[] { _xDoc.DocumentElement, "title", true } ));
         Assert.AreEqual("Test description", po.Invoke("SelectString", new object[] { _xDoc.DocumentElement, "description", true } ));

         try
         {
            po.Invoke("SelectString", new object[] { _xDoc.DocumentElement, "nothing", true });
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
         RssBase r = new RssBase();
         PrivateObject po = new PrivateObject(r);
         Assert.AreEqual(30, po.Invoke("SelectInt", new object[] { _xDoc.DocumentElement, "ttl" }));
      }


      [TestMethod]
      public void SelectDecimalTest()
      {
         RssBase r = new RssBase();
         PrivateObject po = new PrivateObject(r);
         Assert.AreEqual((decimal)3.14, po.Invoke("SelectDecimal", new object[] { _xDoc.DocumentElement, "pi" }));
      }


      [TestMethod]
      public void SelectDateTimeTest()
      {
         RssBase r = new RssBase();
         PrivateObject po = new PrivateObject(r);

         Assert.AreEqual(
            new DateTime(2020, 10, 29, 12, 37, 4, DateTimeKind.Utc),
            po.Invoke("SelectDateTime", new object[] { _xDoc.DocumentElement, "pubDate" }));
      }
   }
}
