using System;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSS;


namespace ServiceTests
{
   [TestClass]
   public class RssControllerTest : ServiceTestBase
   {
      private readonly DataContractJsonSerializerSettings _sSettings =
         new DataContractJsonSerializerSettings() { DateTimeFormat = new DateTimeFormat("o") };


      #region test attributes
      [ClassInitialize()]
      public static void ClassInitialize(TestContext testContext)
      {
         CInitialize();
      }


      [ClassCleanup()]
      public static void ClassCleanup()
      {
         CCleanup();
      }


      [TestCleanup]
      public void TestCleanup()
      {
         TCleanup();
      }
      #endregion


      #region webAPIUnitTests
      [TestMethod]
      public void GetTopItemsTest()
      {
         // valid request
         RSSWebAPISvc.Controllers.RssController svc = new RSSWebAPISvc.Controllers.RssController();
         ItemRow[] rsp = svc.GetTopItems("3");
         Assert.IsNotNull(rsp);
         Assert.AreEqual(3, rsp.Length);

         // invalid request
         try
         {
            svc.GetTopItems("0");
         }
         catch (HttpResponseException he)
         {
            Assert.IsTrue(he.Response.StatusCode == HttpStatusCode.BadRequest);
            Assert.IsTrue(he.Response.ReasonPhrase.Contains("count"));
         }
         catch (Exception)
         {
            Assert.Fail("Should have thrown HttpResponseException");
         }
      }


      [TestMethod]
      public void GetItemsByRangeTest()
      {
         // valid request
         RSSWebAPISvc.Controllers.RssController svc = new RSSWebAPISvc.Controllers.RssController();

         ItemRow[] rsp = svc.GetItemsByRange("2020-10-05T00:00:00Z", "2020-10-06T00:00:00Z");
         Assert.IsNotNull(rsp);
         Assert.AreEqual(1, rsp.Length);

         // invalid request, min date later than max date
         try
         {
            svc.GetItemsByRange("2020-10-05T00:00:00Z", "2020-10-05T00:00:00Z");
         }
         catch (HttpResponseException he)
         {
            Assert.IsTrue(he.Response.StatusCode == HttpStatusCode.BadRequest);
            Assert.IsTrue(he.Response.ReasonPhrase.Contains("less"));
         }
         catch (Exception)
         {
            Assert.Fail("Should have thrown HttpResponseException");
         }
      }


      [TestMethod]
      public void GetItemsByKeywordTest()
      {
         // valid request
         RSSWebAPISvc.Controllers.RssController svc = new RSSWebAPISvc.Controllers.RssController();

         ItemRow[] rsp = svc.GetItemsByKeyword("3");
         Assert.IsNotNull(rsp);
         Assert.AreEqual(1, rsp.Length);

         // invalid request
         try
         {
            svc.GetItemsByKeyword("");
         }
         catch (HttpResponseException he)
         {
            Assert.IsTrue(he.Response.StatusCode == HttpStatusCode.BadRequest);
            Assert.IsTrue(he.Response.ReasonPhrase.Contains("keyword"));
         }
         catch (Exception)
         {
            Assert.Fail("Should have thrown HttpResponseException");
         }
      }


      [TestMethod]
      public void NewHttpResponseExceptionTest()
      {
         RSSWebAPISvc.Controllers.RssController target = new RSSWebAPISvc.Controllers.RssController();
         PrivateObject po = new PrivateObject(target);

         HttpResponseException e = (HttpResponseException)po.Invoke("NewHttpResponseException", new object[] { "Bad!\rBad!\n", HttpStatusCode.NotFound });
         Assert.IsNotNull(e);
         Assert.AreEqual(e.Response.ReasonPhrase, "Bad! Bad! ");
         Assert.AreEqual(HttpStatusCode.NotFound, e.Response.StatusCode);
      }
      #endregion


      /////////////////////////////////////////////////////////////////////////
      //
      // The following are more integration-type tests, and are useful for
      // validating that the service's web.config is configured properly.
      //
      /////////////////////////////////////////////////////////////////////////


      #region webAPIIntegrationTests
      [TestMethod]
      public void WebApiRestGetTopItemsTest()
      {
         StartIis("RSSWebAPISvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         byte[] returnChars = wc.DownloadData($"http://localhost:{IisPort}/api/rss/item?count=3");

         string json = System.Text.Encoding.ASCII.GetString(returnChars);
         Assert.IsNotNull(json);

         var ser = new DataContractJsonSerializer(typeof(ItemRow[]), _sSettings);
         object items = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));
         Assert.IsNotNull(items);
         Assert.AreEqual(3, ((ItemRow[])items).Length);
      }


      [TestMethod]
      public void WebApiRestGetItemsByRangeTest()
      {
         StartIis("RSSWebAPISvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");


         byte[] returnChars = wc.DownloadData($"http://localhost:{IisPort}/api/rss/item?fromDateTime=2020-10-05T00:00:00Z&toDateTime=2020-10-06T00:00:00Z");
         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(ItemRow[]), _sSettings);
         object items = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsNotNull(items);
         Assert.AreEqual(1, ((ItemRow[])items).Length);
      }


      [TestMethod]
      public void WebApiRestGetItemsByKeywordTest()
      {
         StartIis("RSSWebAPISvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");


         byte[] returnChars = wc.DownloadData($"http://localhost:{IisPort}/api/rss/item?keyword=3");
         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(ItemRow[]), _sSettings);
         object items = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsNotNull(items);
         Assert.AreEqual(1, ((ItemRow[])items).Length);
      }
      #endregion
   }
}
