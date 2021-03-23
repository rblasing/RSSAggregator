// Copyright 2020 Richard Blasingame.All rights reserved.

using System;
using System.Configuration;
using System.Data.SqlClient;
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


      /// <summary>
      /// Sample URL: http://sitename/api/rss/top/5
      /// </summary>
      /// <param name="id1">Number of items to return</param>
      [HttpGet]
      [ActionName("top")]
      public ItemRow[] GetTopItems(int id1)
      {
         SqlConnection dbConn = null;

         if (id1 <= 0)
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
               ReasonPhrase = "itemCount must be > 0"
            };
            
            throw new HttpResponseException(msg);
         }

         try
         {
            dbConn = new SqlConnection(_dbConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            ItemRow[] items = dal.GetTopItems(id1);

            return items;
         }
         catch (SqlException)
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
               ReasonPhrase = "Temporary database outage"
            };

            throw new HttpResponseException(msg);
         }
         catch (Exception e)
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
               ReasonPhrase = "Unknown error:" + e.Message
            };

            throw new HttpResponseException(msg);
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
      /// Sample URL: http://sitename/api/rss/search/asteroid
      /// </summary>
      /// <param name="id1">Search term</param>
      /// <returns></returns>
      [HttpGet]
      [ActionName("search")]
      public ItemRow[] GetItemsByKeyword(string id1)
      {
         SqlConnection dbConn = null;

         if (string.IsNullOrWhiteSpace(id1))
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
               ReasonPhrase = "keyword parameter is required"
            };

            throw new HttpResponseException(msg);
         }

         try
         {
            dbConn = new SqlConnection(_dbConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            ItemRow[] items = dal.GetItemsByKeywords(id1);

            return items;
         }
         catch (SqlException)
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
               ReasonPhrase = "Temporary database outage"
            };

            throw new HttpResponseException(msg);
         }
         catch (Exception e)
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
               ReasonPhrase = "Unknown error:" + e.Message
            };

            throw new HttpResponseException(msg);
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
      /// Sample URL: http://sitename/api/rss/search/2020-10-05T00:00:00Z/2020-10-06T00:00:00Z
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
      /// <param name="id1">From date</param>
      /// <param name="id2">To date</param>
      /// <returns></returns>
      [HttpGet]
      [ActionName("search")]
      public ItemRow[] GetItemsByRange(DateTime id1, DateTime id2)
      {
         SqlConnection dbConn = null;

         // the WebAPI framework converts the DateTime parameters to local
         // time, so we need to reset them to UTC
         id1 = id1.ToUniversalTime();
         id2 = id2.ToUniversalTime();

         if (id2 <= id1)
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
               ReasonPhrase = "minDateTime must be less than maxDateTime"
            };

            throw new HttpResponseException(msg);
         }

         try
         {
            dbConn = new SqlConnection(_dbConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            ItemRow[] items = dal.GetItemsByRange(id1, id2);

            return items;
         }
         catch (SqlException)
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
               ReasonPhrase = "Temporary database outage"
            };

            throw new HttpResponseException(msg);
         }
         catch (Exception e)
         {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
               ReasonPhrase = "Unknown error:" + e.Message
            };

            throw new HttpResponseException(msg);
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
   }
}
