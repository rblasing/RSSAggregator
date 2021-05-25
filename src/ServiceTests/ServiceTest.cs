// Copyright 2020 Richard Blasingame.All rights reserved.

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Http;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using blasingame.RSS.xsd;
using RSS;


namespace ServiceTests
{
   [TestClass]
   public class ServiceTest
   {
      private static SqlConnection _dbConn;
      private static Dal _dal;

      private static readonly string IisExpressPath = ConfigurationManager.AppSettings["iisExpressPath"];
      private static readonly string IisPort = ConfigurationManager.AppSettings["iisPort"];
      private static System.Diagnostics.Process _iisProcess;

      private readonly DataContractJsonSerializerSettings _sSettings =
         new DataContractJsonSerializerSettings() { DateTimeFormat = new DateTimeFormat("o") };


      #region test attributes
      [ClassInitialize()]
      public static void ClassInitialize(TestContext testContext)
      {
         string connStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
         _dbConn = new SqlConnection(connStr);
         _dbConn.Open();
         _dal = new Dal(_dbConn);
         InitDb();
      }

      [ClassCleanup()]
      public static void ClassCleanup()
      {
         _dbConn?.Close();
      }


      [TestCleanup]
      public void TestCleanup()
      {
         if (_iisProcess != null  &&  _iisProcess.HasExited == false)
            _iisProcess.Kill();
      }
      #endregion


      private static string GetApplicationPath(string appName)
      {
         var solutionFolder = System.IO.Path.GetDirectoryName(
            System.IO.Path.GetDirectoryName(
               System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));

         return System.IO.Path.Combine(solutionFolder, appName);
      }


      private static void StartIis(string appPath)
      {
         _iisProcess = new System.Diagnostics.Process();
         _iisProcess.StartInfo.FileName = IisExpressPath;
         _iisProcess.StartInfo.Arguments = string.Format("/path:\"{0}\" /port:{1}",
         GetApplicationPath(appPath), IisPort);

         _iisProcess.Start();
      }


      private static void InitDb()
      {
         SqlCommand cmd = new SqlCommand("DELETE FROM rss_item") { Connection = _dbConn };
         cmd.ExecuteNonQuery();

         cmd = new SqlCommand("DELETE FROM rss_feed") { Connection = _dbConn };
         cmd.ExecuteNonQuery();

         _dal.InsertFeed("Test feed 1", "http://testurl1/", false);
         _dal.InsertFeed("Test feed 2", "http://testurl2/", false);
         _dal.InsertFeed("Test feed 3", "http://testurl3/", false);

         ItemRow i1 = new ItemRow()
         {
            Description = "Item 1 desc",
            ItemXml = "<rss><channel><item><title>Item 1 title</title><link>Item 1 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 1, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 1 title",
            Url = "http://item1url/"
         };

         ItemRow i2 = new ItemRow()
         {
            Description = "Item 2 desc",
            ItemXml = "<rss><channel><item><title>Item 2 title</title><link>Item 2 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 2, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 2 title",
            Url = "http://item2url/"
         };

         ItemRow i3 = new ItemRow() {
            Description = "Item 3 desc",
            ItemXml = "<rss><channel><item><title>Item 3 title</title><link>Item 3 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 3, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 3 title",
            Url = "http://item3url/"
         };

         ItemRow i4 = new ItemRow()
         {
            Description = "Item 4 desc",
            ItemXml = "<rss><channel><item><title>Item 4 title</title><link>Item 4 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 4, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 4 title",
            Url = "http://item4url/"
         };

         ItemRow i5 = new ItemRow()
         {
            Description = "Item 5 desc",
            ItemXml = "<rss><channel><item><title>Item 5 title</title><link>Item 5 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 5, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 5 title",
            Url = "http://item5url/"
         };

         ItemRow i6 = new ItemRow()
         {
            Description = "Item 6 desc",
            ItemXml = "<rss><channel><item><title>Item 6 title</title><link>Item 6 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 6, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 6 title",
            Url = "http://item6url/"
         };

         FeedRow[] feed = _dal.GetFeeds();

         _dal.InsertItem(feed[0].FeedId, i1);
         _dal.InsertItem(feed[0].FeedId, i2);

         _dal.InsertItem(feed[1].FeedId, i3);
         _dal.InsertItem(feed[1].FeedId, i4);

         _dal.InsertItem(feed[2].FeedId, i5);
         _dal.InsertItem(feed[2].FeedId, i6);
      }


      #region wcfUnitTests
      [TestMethod]
      public void WcfGetTopItemsTest()
      {
         RSSWCFSvc.RssService svc = new RSSWCFSvc.RssService();

         getTopItemsRequest req = new getTopItemsRequest()
         {
            Body = new getTopItemsRequestBody()
         };

         // valid request
         req.Body.itemCount = 3;
         getItemsResponse rsp = svc.getTopItems(req);

         Assert.IsNotNull(rsp.Body.items);
         Assert.IsNull(rsp.Body.error);
         Assert.AreEqual(3, rsp.Body.items.Count);

         // invalid request
         req.Body.itemCount = 0;
         rsp = svc.getTopItems(req);

         Assert.IsNull(rsp.Body.items);
         Assert.IsNotNull(rsp.Body.error);
         Assert.AreEqual("itemCount", rsp.Body.error.code);
         Assert.AreEqual(false, rsp.Body.error.resubmit);
         Assert.AreEqual(WSErrorType.InvalidArgument, rsp.Body.error.type);
      }


      [TestMethod]
      public void WcfGetItemsByRangeTest()
      {
         RSSWCFSvc.RssService svc = new RSSWCFSvc.RssService();

         getItemsByRangeRequest req = new getItemsByRangeRequest()
         {
            Body = new getItemsByRangeRequestBody()
         };

         // valid request
         req.Body.minDateTime = new DateTime(2020, 10, 5, 0, 0, 0, DateTimeKind.Utc);
         req.Body.maxDateTime = new DateTime(2020, 10, 6, 0, 0, 0, DateTimeKind.Utc);
         getItemsResponse rsp = svc.getItemsByRange(req);

         Assert.IsNotNull(rsp.Body.items);
         Assert.IsNull(rsp.Body.error);
         Assert.AreEqual(1, rsp.Body.items.Count);

         // invalid request, min date later than max date
         req.Body.minDateTime = new DateTime(2020, 10, 6, 0, 0, 0, DateTimeKind.Utc);
         req.Body.maxDateTime = new DateTime(2020, 10, 5, 0, 0, 0, DateTimeKind.Utc);
         rsp = svc.getItemsByRange(req);

         Assert.IsNull(rsp.Body.items);
         Assert.IsNotNull(rsp.Body.error);
         Assert.AreEqual("", rsp.Body.error.code);
         Assert.AreEqual(false, rsp.Body.error.resubmit);
         Assert.AreEqual(WSErrorType.InvalidArgument, rsp.Body.error.type);

         // invalid request, min date is not UTC
         req.Body.minDateTime = new DateTime(2020, 10, 6, 0, 0, 0, DateTimeKind.Local);
         req.Body.maxDateTime = new DateTime(2020, 10, 5, 0, 0, 0, DateTimeKind.Utc);
         rsp = svc.getItemsByRange(req);

         Assert.IsNull(rsp.Body.items);
         Assert.IsNotNull(rsp.Body.error);
         Assert.AreEqual("minDateTime", rsp.Body.error.code);
         Assert.AreEqual(false, rsp.Body.error.resubmit);
         Assert.AreEqual(WSErrorType.InvalidArgument, rsp.Body.error.type);

         // invalid request, max date is not UTC
         req.Body.minDateTime = new DateTime(2020, 10, 6, 0, 0, 0, DateTimeKind.Utc);
         req.Body.maxDateTime = new DateTime(2020, 10, 5, 0, 0, 0, DateTimeKind.Local);
         rsp = svc.getItemsByRange(req);

         Assert.IsNull(rsp.Body.items);
         Assert.IsNotNull(rsp.Body.error);
         Assert.AreEqual("maxDateTime", rsp.Body.error.code);
         Assert.AreEqual(false, rsp.Body.error.resubmit);
         Assert.AreEqual(WSErrorType.InvalidArgument, rsp.Body.error.type);
      }


      [TestMethod]
      public void WcfGetItemsByKeywordTest()
      {
         RSSWCFSvc.RssService svc = new RSSWCFSvc.RssService();

         getItemsByKeywordRequest req = new getItemsByKeywordRequest()
         {
            Body = new getItemsByKeywordRequestBody()
         };

         // valid request
         req.Body.keyword = "3";
         getItemsResponse rsp = svc.getItemsByKeyword(req);
         Assert.IsNotNull(rsp.Body.items);
         Assert.IsNull(rsp.Body.error);
         Assert.AreEqual(1, rsp.Body.items.Count);

         // invalid request
         req.Body.keyword = "";
         rsp = svc.getItemsByKeyword(req);

         Assert.IsNull(rsp.Body.items);
         Assert.IsNotNull(rsp.Body.error);
         Assert.AreEqual("keyword", rsp.Body.error.code);
         Assert.AreEqual(false, rsp.Body.error.resubmit);
         Assert.AreEqual(WSErrorType.InvalidArgument, rsp.Body.error.type);
      }
      #endregion


      #region webAPIUnitTests
      [TestMethod]
      public void WebApiGetTopItemsTest()
      {
         // valid request
         RSSWebAPISvc.Controllers.RssController svc = new RSSWebAPISvc.Controllers.RssController();
         ItemRow[] rsp = svc.GetTopItems(3);
         Assert.IsNotNull(rsp);
         Assert.AreEqual(3, rsp.Length);

         // invalid request
         try
         {
            svc.GetTopItems(0);
         }
         catch (HttpResponseException he)
         {
            Assert.IsTrue(he.Response.StatusCode == HttpStatusCode.BadRequest);
            Assert.IsTrue(he.Response.ReasonPhrase.Contains("itemCount"));
         }
         catch (Exception)
         {
            Assert.Fail("Should have thrown HttpResponseException");
         }
      }


      [TestMethod]
      public void WebApiGetItemsByRangeTest()
      {
         // valid request
         RSSWebAPISvc.Controllers.RssController svc = new RSSWebAPISvc.Controllers.RssController();

         ItemRow[] rsp = svc.GetItemsByRange(
            new DateTime(2020, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2020, 10, 6, 0, 0, 0, 0, DateTimeKind.Utc));

         Assert.IsNotNull(rsp);
         Assert.AreEqual(1, rsp.Length);

         // invalid request, min date later than max date
         try
         {
            svc.GetItemsByRange(
               new DateTime(2020, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc),
               new DateTime(2020, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc));
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
      public void WebApiGetItemsByKeywordTest()
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
      #endregion


      /////////////////////////////////////////////////////////////////////////
      //
      // The following are more integration-type tests, and are useful for
      // validating that the service's web.config is configured properly.
      //
      /////////////////////////////////////////////////////////////////////////

      #region wcfIntegrationTests
      [TestMethod]
      public void WcfJsonGetTopItemsTest()
      {
         StartIis("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         byte[] returnChars = wc.UploadData($"http://localhost:{IisPort}/RSSService.svc/json/getTopItems",
            System.Text.Encoding.ASCII.GetBytes("{\"itemCount\": 3}"));

         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(getItemsResponseBody));
         object body = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));
         Assert.IsTrue(body != null);
         Assert.AreEqual(3, ((getItemsResponseBody)body).items.Count);
         System.Console.WriteLine(((getItemsResponseBody)body).items.Count);
      }


      [TestMethod]
      public void WcfJsonGetItemsByRangeTest()
      {
         StartIis("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         DateTime testDate = new DateTime(2020, 10, 5, 8, 20, 0, DateTimeKind.Utc);

         long jsTestDate = (long)Math.Round((double)(testDate.Ticks - 621355968000000000) / 10000);
         long jsTestDatePlusOneHour = (long)Math.Round((double)(testDate.AddHours(1).Ticks - 621355968000000000) / 10000);

         byte[] returnChars = wc.UploadData($"http://localhost:{IisPort}/RSSService.svc/json/getItemsByRange",
            System.Text.Encoding.ASCII.GetBytes("{\"minDateTime\":\"/Date(" +
               jsTestDate.ToString() + ")/\",\"maxDateTime\":\"/Date(" +
               jsTestDatePlusOneHour.ToString() + ")/\"}"));

         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(getItemsResponseBody));
         object body = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsTrue(body != null);
         Assert.AreEqual(1, ((getItemsResponseBody)body).items.Count);
      }


      [TestMethod]
      public void WcfJsonGetItemsByKeywordTest()
      {
         StartIis("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         byte[] returnChars = wc.UploadData($"http://localhost:{IisPort}/RSSService.svc/json/getItemsByKeyword",
            System.Text.Encoding.ASCII.GetBytes("{\"keyword\":\"3\"}"));

         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(getItemsResponseBody));
         object body = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsTrue(body != null);
         Assert.AreEqual(1, ((getItemsResponseBody)body).items.Count);
      }


      [TestMethod]
      public void WcfSoapGetTopItemsTest()
      {
         StartIis("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "text/xml");
         wc.Headers.Add("SOAPAction", "http://blasingame/getTopItems");
         wc.UseDefaultCredentials = true;

         byte[] returnChars = wc.UploadData($"http://localhost:{IisPort}/RSSService.svc",
            System.Text.Encoding.UTF8.GetBytes(
@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
<s:Body>
   <getTopItemsRequest xmlns=""http://blasingame/RSS.xsd"">
      <itemCount>3</itemCount>
   </getTopItemsRequest>
</s:Body>
</s:Envelope>"));

         string soapRsp = System.Text.Encoding.UTF8.GetString(returnChars);

         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(soapRsp);
         XmlNamespaceManager ns = new XmlNamespaceManager(xDoc.NameTable);
         ns.AddNamespace("ns", "http://blasingame/RSS.xsd");
         XmlNode items = xDoc.SelectSingleNode("//ns:items", ns);
         Assert.AreEqual(3, items?.ChildNodes.Count);
      }


      [TestMethod]
      public void WcfSoapGetItemsByRangeTest()
      {
         StartIis("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "text/xml");
         wc.Headers.Add("SOAPAction", "http://blasingame/getItemsByRange");
         wc.UseDefaultCredentials = true;

         byte[] returnChars = wc.UploadData($"http://localhost:{IisPort}/RSSService.svc",
            System.Text.Encoding.UTF8.GetBytes(
@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
      <s:Body>
         <getItemsByRangeRequest xmlns=""http://blasingame/RSS.xsd"">
            <minDateTime>2020-10-05T00:00:00Z</minDateTime>
            <maxDateTime>2020-10-06T00:00:00Z</maxDateTime>
         </getItemsByRangeRequest>
      </s:Body>
   </s:Envelope>"));

         string soapRsp = System.Text.Encoding.UTF8.GetString(returnChars);

         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(soapRsp);
         XmlNamespaceManager ns = new XmlNamespaceManager(xDoc.NameTable);
         ns.AddNamespace("ns", "http://blasingame/RSS.xsd");
         XmlNode items = xDoc.SelectSingleNode("//ns:items", ns);
         Assert.AreEqual(1, items?.ChildNodes.Count);
      }


      [TestMethod]
      public void WcfSoapGetItemsByKeywordTest()
      {
         StartIis("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "text/xml");
         wc.Headers.Add("SOAPAction", "http://blasingame/getItemsByKeyword");
         wc.UseDefaultCredentials = true;

         byte[] returnChars = wc.UploadData($"http://localhost:{IisPort}/RSSService.svc",
            System.Text.Encoding.UTF8.GetBytes(
@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
      <s:Body>
         <getItemsByKeywordRequest xmlns=""http://blasingame/RSS.xsd"">
            <keyword>3</keyword>
         </getItemsByKeywordRequest>
      </s:Body>
   </s:Envelope>"));

         string soapRsp = System.Text.Encoding.UTF8.GetString(returnChars);

         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(soapRsp);
         XmlNamespaceManager ns = new XmlNamespaceManager(xDoc.NameTable);
         ns.AddNamespace("ns", "http://blasingame/RSS.xsd");
         XmlNode items = xDoc.SelectSingleNode("//ns:items", ns);
         Assert.AreEqual(1, items?.ChildNodes.Count);
      }
      #endregion


      #region webAPIIntegrationTests
      [TestMethod]
      public void WebApiRestGetTopItemsTest()
      {
         StartIis("RSSWebAPISvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         byte[] returnChars = wc.DownloadData($"http://localhost:{IisPort}/api/rss/top/3");

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


         byte[] returnChars = wc.DownloadData($"http://localhost:{IisPort}/api/rss/search/2020-10-05T00:00:00Z/2020-10-06T00:00:00Z");
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


         byte[] returnChars = wc.DownloadData($"http://localhost:{IisPort}/api/rss/search/3");
         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(ItemRow[]), _sSettings);
         object items = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsNotNull(items);
         Assert.AreEqual(1, ((ItemRow[])items).Length);
      }
      #endregion
   }
}
