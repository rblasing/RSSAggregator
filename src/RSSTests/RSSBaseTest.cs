// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace RSSTests
{
   [TestClass]
   public class RSSBaseTest : TestBase
   {
      private XmlDocument xDoc = new XmlDocument();


      public RSSBaseTest()
      {
         xDoc.LoadXml(testItemNode);
      }



      [TestMethod]
      public void selectStringTest()
      {
         RSSBase r = new RSSBase();
         PrivateObject po = new PrivateObject(r);

         Assert.AreEqual("Test title", po.Invoke("selectString", new object[] { xDoc.DocumentElement, "title", true } ));
         Assert.AreEqual("Test description", po.Invoke("selectString", new object[] { xDoc.DocumentElement, "description", true } ));

         try
         {
            po.Invoke("selectString", new object[] { xDoc.DocumentElement, "nothing", true });
            Assert.Fail("RSSBase.selectString(node, 'nothing', true) should have thrown RSSException");
         }
         catch (RSSException)
         {
         }
         catch (Exception)
         {
            Assert.Fail("RSSBase.selectString(node, 'nothing', true) should have thrown RSSException");
         }
      }


      [TestMethod]
      public void selectIntTest()
      {
         RSSBase r = new RSSBase();
         PrivateObject po = new PrivateObject(r);
         Assert.AreEqual(30, po.Invoke("selectInt", new object[] { xDoc.DocumentElement, "ttl" }));
      }


      [TestMethod]
      public void selectDecimalTest()
      {
         RSSBase r = new RSSBase();
         PrivateObject po = new PrivateObject(r);
         Assert.AreEqual((decimal)3.14, po.Invoke("selectDecimal", new object[] { xDoc.DocumentElement, "pi" }));
      }


      [TestMethod]
      public void selectDateTimeTest()
      {
         RSSBase r = new RSSBase();
         PrivateObject po = new PrivateObject(r);

         Assert.AreEqual(
            new DateTime(2020, 10, 29, 12, 37, 4, DateTimeKind.Utc),
            po.Invoke("selectDateTime", new object[] { xDoc.DocumentElement, "pubDate" }));
      }
   }
}
