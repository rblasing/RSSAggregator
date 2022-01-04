using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using RSS;


namespace RSSWebAPISvc.Controllers
{
   public class RssController : ApiController
   {
      private readonly string _dbConnStr =
         ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;

      private int throttleRate =
         Convert.ToInt32(ConfigurationManager.AppSettings["throttleRate"]);

      private int retrySec;
      private string errorMsg;


      [HttpGet]
      [ActionName("item")]
      [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      public ItemRow[] Get()
      {
         string key = System.Web.HttpContext.Current?.Request?.UserHostAddress;

         if (key != null  &&
            Util.WSBase.ShouldThrottle(key, throttleRate, out retrySec, out errorMsg))
         {
            log4net.LogManager.GetLogger("RssController").Warn(errorMsg);
 
            throw NewHttpResponseException(
               "Throttling has been activated for this connection. Retry in " +
               retrySec.ToString() + " seconds.",
               HttpStatusCode.ServiceUnavailable);
         }

         var countPair = Request.GetQueryNameValuePairs().Where(p => p.Key == "count");
         var keywordPair = Request.GetQueryNameValuePairs().Where(p => p.Key == "keyword");
         var fromDatePair = Request.GetQueryNameValuePairs().Where(p => p.Key == "fromDateTime");
         var toDatePair = Request.GetQueryNameValuePairs().Where(p => p.Key == "toDateTime");

         if (countPair.Count() > 0)
            return GetTopItems(countPair.First().Value);

         if (keywordPair.Count() > 0)
            return GetItemsByKeyword(keywordPair.First().Value);

         if (fromDatePair.Count() > 0)
         {
            string toDate = (toDatePair != null  &&  toDatePair.Count() > 0) ? 
               toDatePair.First().Value : null;

            return GetItemsByRange(fromDatePair.First().Value, toDate);
         }

         throw NewHttpResponseException("count, keyword, or fromDate/toDate parameters must be specified",
            HttpStatusCode.BadRequest);
      }


      /// <summary>
      /// Sample URL: http://sitename/api/rss/item?count=5
      /// </summary>
      /// <param name="sCount">Number of items to return</param>
      public ItemRow[] GetTopItems(string sCount)
      {
         SqlConnection dbConn = null;

         if (!int.TryParse(sCount, out int count))
         {
            throw NewHttpResponseException("count parameter must be an integer",
               HttpStatusCode.BadRequest);
         }

         if (count <= 0)
         {
            throw NewHttpResponseException("count parameter must be > 0",
               HttpStatusCode.BadRequest);
         }

         try
         {
            dbConn = new SqlConnection(_dbConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            var items = dal.GetTopItems(count);

            return (items?.ToArray());
         }
         catch (SqlException)
         {
            throw NewHttpResponseException("Temporary database outage",
               HttpStatusCode.ServiceUnavailable);
         }
         catch (Exception e)
         {
            throw NewHttpResponseException("Unknown error: " + e.Message,
               HttpStatusCode.ServiceUnavailable);
         }
         finally
         {
            if (dbConn != null)
            {
               dbConn.Close();
               dbConn.Dispose();
               dbConn = null;
            }
         }
      }


      /// <summary>
      /// Sample URL: http://sitename/api/rss/item?keyword=asteroid
      /// </summary>
      /// <param name="keyword">Search term</param>
      public ItemRow[] GetItemsByKeyword(string keyword)
      {
         SqlConnection dbConn = null;

         if (string.IsNullOrWhiteSpace(keyword))
         {
            throw NewHttpResponseException("keyword parameter is required",
               HttpStatusCode.BadRequest);
         }

         try
         {
            dbConn = new SqlConnection(_dbConnStr);
            dbConn.Open();
            DalQuery dal = new DalQuery(dbConn);
            var items = dal.GetItemsByKeywords(keyword);

            return (items?.ToArray());
         }
         catch (SqlException)
         {
            throw NewHttpResponseException("Temporary database outage",
               HttpStatusCode.ServiceUnavailable);
         }
         catch (Exception e)
         {
            throw NewHttpResponseException("Unknown error:" + e.Message,
               HttpStatusCode.ServiceUnavailable);
         }
         finally
         {
            if (dbConn != null)
            {
               dbConn.Close();
               dbConn.Dispose();
               dbConn = null;
            }
         }
      }


      /// <summary>
      /// Sample URL: http://sitename/api/rss/item?fromDateTime=2020-10-05T00:00:00Z&toDateTime=2020-10-06T00:00:00Z
      ///
      /// Note: WebAPI routing doesn't like periods in a URL, so don't include
      /// milliseconds when addressing this method.  Dates in the form of
      /// yyyy-mm-ddThh:mm:ssZ should work fine, as long as the web.config is
      /// configured to allow colons by adding <c>requestPathInvalidCharacters=""</c>
      /// 
      /// <system.web>
      ///   <httpRuntime requestPathInvalidCharacters=""/>
      /// </system.web>
      /// 
      /// </summary>
      /// <param name="sFromDate">From date</param>
      /// <param name="sToDate">To date</param>
      public ItemRow[] GetItemsByRange(string sFromDate, string sToDate)
      {
         SqlConnection dbConn = null;

         bool fromOk = DateTime.TryParse(sFromDate, out DateTime fromDateTime);
         bool toOk = DateTime.TryParse(sToDate, out DateTime toDateTime);
         fromDateTime = fromDateTime.ToUniversalTime();
         toDateTime = toDateTime.ToUniversalTime();

         if (!fromOk)
         {
            throw NewHttpResponseException("Unable to parse fromDateTime",
               HttpStatusCode.BadRequest);
         }

         if (!toOk  &&  !string.IsNullOrWhiteSpace(sToDate))
         {
            throw NewHttpResponseException("Unable to parse toDateTime",
               HttpStatusCode.BadRequest);
         }

         if (string.IsNullOrWhiteSpace(sToDate))
            toDateTime = DateTime.UtcNow;

         if (fromDateTime >= toDateTime)
         {
            throw NewHttpResponseException("fromDateTime must be less than toDateTime",
               HttpStatusCode.BadRequest);
         }

         try
         {
            dbConn = new SqlConnection(_dbConnStr);
            dbConn.Open();
            DalQuery dal = new DalQuery(dbConn);
            var items = dal.GetItemsByRange(fromDateTime, toDateTime);

            return (items?.ToArray());
         }
         catch (SqlException)
         {
            throw NewHttpResponseException("Temporary database outage",
               HttpStatusCode.ServiceUnavailable);
         }
         catch (Exception e)
         {
            throw NewHttpResponseException("Unknown error:" + e.Message,
               HttpStatusCode.ServiceUnavailable);
         }
         finally
         {
            if (dbConn != null)
            {
               dbConn.Close();
               dbConn.Dispose();
               dbConn = null;
            }
         }
      }


      private HttpResponseException NewHttpResponseException(
         string reason, HttpStatusCode code)
      {
         HttpResponseMessage msg = new HttpResponseMessage(code)
         {
            ReasonPhrase = reason.Replace("\n", " ").Replace("\r", " ")
         };

         return new HttpResponseException(msg);
      }
   }
}
