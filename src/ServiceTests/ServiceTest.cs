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
      private static SqlConnection dbConn = null;
      private static DAL dal = null;

      private static string iisExpressPath = ConfigurationManager.AppSettings["iisExpressPath"];
      private static string iisPort = ConfigurationManager.AppSettings["iisPort"];
      private static System.Diagnostics.Process iisProcess;

      DataContractJsonSerializerSettings sSettings =
         new DataContractJsonSerializerSettings() { DateTimeFormat = new DateTimeFormat("o") };


      #region test attributes
      [ClassInitialize()]
      public static void ClassInitialize(TestContext testContext)
      {
         string connStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
         dbConn = new SqlConnection(connStr);
         dbConn.Open();
         dal = new DAL(dbConn);
         InitDB();
      }

      [ClassCleanup()]
      public static void ClassCleanup()
      {
         if (dbConn != null)
            dbConn.Close();
      }


      [TestCleanup]
      public void TestCleanup()
      {
         if (iisProcess != null  &&  iisProcess.HasExited == false)
            iisProcess.Kill();
      }
      #endregion


      protected static string GetApplicationPath(string appName)
      {
         var solutionFolder = System.IO.Path.GetDirectoryName(
            System.IO.Path.GetDirectoryName(
               System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));

         return System.IO.Path.Combine(solutionFolder, appName);
      }


      private static void StartIIS(string appPath)
      {
         iisProcess = new System.Diagnostics.Process();
         iisProcess.StartInfo.FileName = iisExpressPath;

         iisProcess.StartInfo.Arguments = string.Format("/path:\"{0}\" /port:{1}",
            GetApplicationPath(appPath), iisPort);

         iisProcess.Start();
      }


      private static void InitDB()
      {
         SqlCommand cmd = new SqlCommand("DELETE FROM rss_item");
         cmd.Connection = dbConn;
         cmd.ExecuteNonQuery();

         cmd = new SqlCommand("DELETE FROM rss_feed");
         cmd.Connection = dbConn;
         cmd.ExecuteNonQuery();

         dal.insertFeed("Test feed 1", "http://testurl1/", false);
         dal.insertFeed("Test feed 2", "http://testurl2/", false);
         dal.insertFeed("Test feed 3", "http://testurl3/", false);

         ItemRow i1 = new ItemRow();
         i1.description = "Item 1 desc";
         i1.itemXml = "<rss><channel><item><title>Item 1 title</title><link>Item 1 link</link></item></channel></rss>";
         i1.pubDate = new DateTime(2020, 10, 1, 8, 20, 0, DateTimeKind.Utc);
         i1.title = "Item 1 title";
         i1.url = "http://item1url/";

         ItemRow i2 = new ItemRow();
         i2.description = "Item 2 desc";
         i2.itemXml = "<rss><channel><item><title>Item 2 title</title><link>Item 2 link</link></item></channel></rss>";
         i2.pubDate = new DateTime(2020, 10, 2, 8, 20, 0, DateTimeKind.Utc);
         i2.title = "Item 2 title";
         i2.url = "http://item2url/";

         ItemRow i3 = new ItemRow();
         i3.description = "Item 3 desc";
         i3.itemXml = "<rss><channel><item><title>Item 3 title</title><link>Item 3 link</link></item></channel></rss>";
         i3.pubDate = new DateTime(2020, 10, 3, 8, 20, 0, DateTimeKind.Utc);
         i3.title = "Item 3 title";
         i3.url = "http://item3url/";

         ItemRow i4 = new ItemRow();
         i4.description = "Item 4 desc";
         i4.itemXml = "<rss><channel><item><title>Item 4 title</title><link>Item 4 link</link></item></channel></rss>";
         i4.pubDate = new DateTime(2020, 10, 4, 8, 20, 0, DateTimeKind.Utc);
         i4.title = "Item 4 title";
         i4.url = "http://item4url/";

         ItemRow i5 = new ItemRow();
         i5.description = "Item 5 desc";
         i5.itemXml = "<rss><channel><item><title>Item 5 title</title><link>Item 5 link</link></item></channel></rss>";
         i5.pubDate = new DateTime(2020, 10, 5, 8, 20, 0, DateTimeKind.Utc);
         i5.title = "Item 5 title";
         i5.url = "http://item5url/";

         ItemRow i6 = new ItemRow();
         i6.description = "Item 6 desc";
         i6.itemXml = "<rss><channel><item><title>Item 6 title</title><link>Item 6 link</link></item></channel></rss>";
         i6.pubDate = new DateTime(2020, 10, 6, 8, 20, 0, DateTimeKind.Utc);
         i6.title = "Item 6 title";
         i6.url = "http://item6url/";

         FeedRow[] feed = dal.getFeeds();

         dal.insertItem(feed[0].feedId, i1);
         dal.insertItem(feed[0].feedId, i2);

         dal.insertItem(feed[1].feedId, i3);
         dal.insertItem(feed[1].feedId, i4);

         dal.insertItem(feed[2].feedId, i5);
         dal.insertItem(feed[2].feedId, i6);
      }


      #region wcfUnitTests
      [TestMethod]
      public void wcfGetTopItemsTest()
      {
         RSSWCFSvc.RSSService svc = new RSSWCFSvc.RSSService();

         getTopItemsRequest req = new getTopItemsRequest();
         req.Body = new getTopItemsRequestBody();

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
      public void wcfGetItemsByRangeTest()
      {
         RSSWCFSvc.RSSService svc = new RSSWCFSvc.RSSService();

         getItemsByRangeRequest req = new getItemsByRangeRequest();
         req.Body = new getItemsByRangeRequestBody();

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
      public void wcfGetItemsByKeywordTest()
      {
         RSSWCFSvc.RSSService svc = new RSSWCFSvc.RSSService();

         getItemsByKeywordRequest req = new getItemsByKeywordRequest();
         req.Body = new getItemsByKeywordRequestBody();

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
      public void webAPIGetTopItemsTest()
      {
         // valid request
         RSSWebAPISvc.Controllers.RSSController svc = new RSSWebAPISvc.Controllers.RSSController();
         ItemRow[] rsp = svc.getTopItems(3);
         Assert.IsNotNull(rsp);
         Assert.AreEqual(3, rsp.Length);

         // invalid request
         try
         {
            rsp = svc.getTopItems(0);
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
      public void webAPIGetItemsByRangeTest()
      {
         // valid request
         RSSWebAPISvc.Controllers.RSSController svc = new RSSWebAPISvc.Controllers.RSSController();

         ItemRow[] rsp = svc.getItemsByRange(
            new DateTime(2020, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2020, 10, 6, 0, 0, 0, 0, DateTimeKind.Utc));

         Assert.IsNotNull(rsp);
         Assert.AreEqual(1, rsp.Length);

         // invalid request, min date later than max date
         try
         {
            rsp = svc.getItemsByRange(
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
      public void webAPIGetItemsByKeywordTest()
      {
         // valid request
         RSSWebAPISvc.Controllers.RSSController svc = new RSSWebAPISvc.Controllers.RSSController();

         ItemRow[] rsp = svc.getItemsByKeyword("3");
         Assert.IsNotNull(rsp);
         Assert.AreEqual(1, rsp.Length);

         // invalid request
         try
         {
            rsp = svc.getItemsByKeyword("");
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
      public void wcfJsonGetTopItemsTest()
      {
         StartIIS("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         byte[] returnChars = wc.UploadData($"http://localhost:{iisPort}/RSSService.svc/json/getTopItems",
            System.Text.Encoding.ASCII.GetBytes("{\"itemCount\": 3}"));

         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(getItemsResponseBody));
         object body = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsTrue(body != null);
         Assert.AreEqual(3, ((getItemsResponseBody)body).items.Count);
      }


      [TestMethod]
      public void wcfJsonGetItemsByRangeTest()
      {
         StartIIS("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         DateTime testDate = new DateTime(2020, 10, 5, 8, 20, 0, DateTimeKind.Utc);

         long jsTestDate = (long)Math.Round((double)(testDate.Ticks - 621355968000000000) / 10000);
         long jsTestDatePlusOneHour = (long)Math.Round((double)(testDate.AddHours(1).Ticks - 621355968000000000) / 10000);

         byte[] returnChars = wc.UploadData($"http://localhost:{iisPort}/RSSService.svc/json/getItemsByRange",
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
      public void wcfJsonGetItemsByKeywordTest()
      {
         StartIIS("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         byte[] returnChars = wc.UploadData($"http://localhost:{iisPort}/RSSService.svc/json/getItemsByKeyword",
            System.Text.Encoding.ASCII.GetBytes("{\"keyword\":\"3\"}"));

         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(getItemsResponseBody));
         object body = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsTrue(body != null);
         Assert.AreEqual(1, ((getItemsResponseBody)body).items.Count);
      }


      [TestMethod]
      public void wcfSoapGetTopItemsTest()
      {
         StartIIS("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "text/xml");
         wc.Headers.Add("SOAPAction", "http://blasingame/getTopItems");
         wc.UseDefaultCredentials = true;

         byte[] returnChars = wc.UploadData($"http://localhost:{iisPort}/RSSService.svc",
            System.Text.UTF8Encoding.UTF8.GetBytes(
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
         Assert.AreEqual(3, items.ChildNodes.Count);
      }


      [TestMethod]
      public void wcfSoapGetItemsByRangeTest()
      {
         StartIIS("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "text/xml");
         wc.Headers.Add("SOAPAction", "http://blasingame/getItemsByRange");
         wc.UseDefaultCredentials = true;

         byte[] returnChars = wc.UploadData($"http://localhost:{iisPort}/RSSService.svc",
            System.Text.UTF8Encoding.UTF8.GetBytes(
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
         Assert.AreEqual(1, items.ChildNodes.Count);
      }


      [TestMethod]
      public void wcfSoapGetItemsByKeywordTest()
      {
         StartIIS("RSSWCFSvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "text/xml");
         wc.Headers.Add("SOAPAction", "http://blasingame/getItemsByKeyword");
         wc.UseDefaultCredentials = true;

         byte[] returnChars = wc.UploadData($"http://localhost:{iisPort}/RSSService.svc",
            System.Text.UTF8Encoding.UTF8.GetBytes(
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
         Assert.AreEqual(1, items.ChildNodes.Count);
      }
      #endregion


      #region webAPIIntegrationTests
      [TestMethod]
      public void webAPIRestGetTopItemsTest()
      {
         StartIIS("RSSWebAPISvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");

         byte[] returnChars = wc.DownloadData($"http://localhost:{iisPort}/api/rss/top/3");

         string json = System.Text.Encoding.ASCII.GetString(returnChars);
         Assert.IsNotNull(json);

         var ser = new DataContractJsonSerializer(typeof(ItemRow[]), sSettings);
         object items = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));
         Assert.IsNotNull(items);
         Assert.AreEqual(3, ((ItemRow[])items).Length);
      }


      [TestMethod]
      public void webAPIRestGetItemsByRangeTest()
      {
         StartIIS("RSSWebAPISvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");


         byte[] returnChars = wc.DownloadData($"http://localhost:{iisPort}/api/rss/search/2020-10-05T00:00:00Z/2020-10-06T00:00:00Z");
         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(ItemRow[]), sSettings);
         object items = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsNotNull(items);
         Assert.AreEqual(1, ((ItemRow[])items).Length);
      }


      [TestMethod]
      public void webAPIRestGetItemsByKeywordTest()
      {
         StartIIS("RSSWebAPISvc");

         WebClient wc = new WebClient();
         wc.Headers.Add("Content-Type", "application/json");


         byte[] returnChars = wc.DownloadData($"http://localhost:{iisPort}/api/rss/search/3");
         string json = System.Text.Encoding.ASCII.GetString(returnChars);

         var ser = new DataContractJsonSerializer(typeof(ItemRow[]), sSettings);
         object items = ser.ReadObject(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

         Assert.IsNotNull(items);
         Assert.AreEqual(1, ((ItemRow[])items).Length);
      }
      #endregion
   }
}
