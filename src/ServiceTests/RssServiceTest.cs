using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using System.Net;
using System.Runtime.Serialization.Json;

using blasingame.RSS.xsd;
using RSS;
using System.Collections.Generic;

namespace ServiceTests
{
   [TestClass]
   public class RssServiceTest : ServiceTestBase
   {
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


      #region wcfUnitTests
      [TestMethod]
      public void getTopItemsTest()
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
      public void getItemsByRangeTest()
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
      public void getItemsByKeywordTest()
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


      [TestMethod]
      public void BuildItemListTest()
      {
         ItemRow i1 = new ItemRow()
         {
            Description = "Item 1 desc",
            FeedName = "Feed1",
            ItemXml = "<rss><channel><item><title>Item 1 title</title><link>Item 1 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 1, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 1 title",
            Url = "http://item1url/"
         };

         ItemRow i2 = new ItemRow()
         {
            Description = "Item 2 desc",
            FeedName = "Feed2",
            ItemXml = "<rss><channel><item><title>Item 2 title</title><link>Item 2 link</link></item></channel></rss>",
            PubDate = new DateTime(2020, 10, 2, 8, 20, 0, DateTimeKind.Utc),
            Title = "Item 2 title",
            Url = "http://item2url/"
         };

         List<ItemRow> inList = new List<ItemRow>();

         // null parameter
         PrivateType pt = new PrivateType(typeof(RSSWCFSvc.RssService));
         ItemList outList = (ItemList)pt.InvokeStatic("BuildItemList", new object[] { null });
         Assert.IsNull(outList);

         // empty list
         outList = (ItemList)pt.InvokeStatic("BuildItemList", new object[] { inList });
         Assert.IsNull(outList);

         // populated list
         inList.Add(i1);
         inList.Add(i2);
         outList = (ItemList)pt.InvokeStatic("BuildItemList", new object[] { inList });
         Assert.IsNotNull(outList);
         Assert.AreEqual(2, outList.Count);

         Assert.AreEqual(i1.Description, outList[0].description);
         Assert.AreEqual(i1.FeedName, outList[0].publisher);
         Assert.AreEqual(i1.PubDate, outList[0].pubDate);
         Assert.AreEqual(i1.Title, outList[0].title);
         Assert.AreEqual(i1.Url, outList[0].link.AbsoluteUri);

         Assert.AreEqual(i2.Description, outList[1].description);
         Assert.AreEqual(i2.FeedName, outList[1].publisher);
         Assert.AreEqual(i2.PubDate, outList[1].pubDate);
         Assert.AreEqual(i2.Title, outList[1].title);
         Assert.AreEqual(i2.Url, outList[1].link.AbsoluteUri);
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
   }
}
