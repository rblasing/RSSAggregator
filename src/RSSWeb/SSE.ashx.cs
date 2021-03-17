// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using RSS;
using Util;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]


namespace Portfolio
{
   /// <summary>
   /// https://html.spec.whatwg.org/multipage/server-sent-events.html
   /// </summary>
   public class SSE : IHttpHandler
   {
      // the frequency at which any new items should be pushed to the client
      private static readonly int sleepSecs = int.Parse((ConfigurationManager.AppSettings["sseSleepSec"]));

      // max number of items to push to the client in a single message
      private static readonly int maxItems = int.Parse((ConfigurationManager.AppSettings["sseMaxItems"]));

      // hourly range to use when searching for state-specific items
      private static readonly int heatMapHours = int.Parse((ConfigurationManager.AppSettings["heatMapHours"]));

      private static readonly string connStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("SSE");


      public void ProcessRequest(HttpContext context)
      {
         context.Response.CacheControl = "no-cache";
         context.Response.ContentType = "text/event-stream";
         context.Response.Expires = -1;
         context.Response.StatusCode = 200;

         SqlConnection dbConn = null;
         DAL dal = null;

         try
         {
            DateTime lastStamp = DateTime.MinValue;
            string lastEventIdHeader = context.Request.Headers["Last-Event-Id"];
            string lastEventIdParam  = context.Request.Params["Last-Event-Id"];
            long lastTicks = 0;
            bool isFirstConnect = true;

            // if the client connection drops, then it will send this
            // header when it reconnects.  It can be used to send catch-up
            // messages to the client, as we'll do below.
            if (!string.IsNullOrEmpty(lastEventIdHeader)  &&
               long.TryParse(lastEventIdHeader, out lastTicks))
            {
               logger?.Debug("Last-Event-Id: " + lastEventIdHeader);
            }

            // the client might also sent an event ID on the initial connection
            // as a parameter, so that it can catch up from an extended
            // disconnection
            if (!string.IsNullOrEmpty(lastEventIdParam)  &&
               long.TryParse(lastEventIdParam, out lastTicks))
            {
               logger?.Debug("Last-Event-Id: " + lastEventIdHeader);
            }

            bool isStateReq = context.Request.Params["stateHeatData"] != null;
            string prevStateMsg = string.Empty;

            while (true)
            {
               if (isFirstConnect)
               {
                  isFirstConnect = false;
                  context.Response.Write(":");
                  context.Response.Flush();
                  continue;
               }
               
               if (dbConn == null)
               {
                  dbConn = new SqlConnection(connStr);
                  dal = new DAL(dbConn);
               }

               if (dbConn.State != System.Data.ConnectionState.Open)
                  dbConn.Open();

               // client reconnected and sent a Last-Event-ID, so use that to
               // send a catch-up message
               if (lastTicks > 0)
               {
                  lastStamp = JS.DateFromJSTicks(lastTicks);
                  lastTicks = 0;
               }
               else if (lastStamp == DateTime.MinValue)
                  lastStamp = dal.getMaxInsertDate();

               if (isStateReq)
               {
                  // get latest stats on items which contain a state name
                  Dictionary<string, int> stateTotals =
                     dal.getStateActivityByRange(heatMapHours);

                  if (stateTotals != null  &&  stateTotals.Count > 0)
                  {
                     string msg = buildJsonResponse(stateTotals);

                     // don't send a message if the stats haven't changed
                     if (msg != prevStateMsg)
                     {
                        context.Response.Write(msg.Replace("|||ID|||",
                           JS.JSTicksFromDate(DateTime.UtcNow).ToString()));
                     }
                     else
                        context.Response.Write("data: nodata\n\n");

                     prevStateMsg = msg;
                  }
                  else
                     context.Response.Write("data: nodata\n\n");
               }
               else
               {
                  // get maxItems newer than the last ones sent to the client
                  ItemRow[] items = dal.getNewItems(lastStamp, maxItems);

                  if (items != null)
                  {
                     logger?.Info("New item count: " + items.Length);

                     // make a note of the latest item being sent, so that only
                     // newer ones will be sent next time
                     lastStamp = items.Max(i => i.insDate);

                     // build the message and send it
                     context.Response.Write(
                        buildJsonResponse(JS.JSTicksFromDate(lastStamp).ToString(), items));
                  }
                  else
                  {
                     // even though there are no new items, send a comment to
                     // keep the connection alive
                     context.Response.Write("data: nodata\n\n");
                  }
               }

               dbConn.Close();
               context.Response.Flush();
               System.Threading.Thread.Sleep(sleepSecs * 1000);
            }
         }
         catch (HttpException he)
         {
            logger?.Error($"HttpException: {he.Message} : {he.ErrorCode}");
         }
         catch (Exception e)
         {
            context.Response.Write("data: nodata\n\n");
            context.Response.Flush();

            logger?.Error($"Unhandled exception: {e.Message} : {e.StackTrace}");
         }
         finally
         {
            if (dbConn != null)
            {
               if (dbConn.State != System.Data.ConnectionState.Closed)
                  dbConn.Close();

               dbConn.Dispose();
            }
         }
      }


      private string buildJsonResponse(string id, ItemRow[] items)
      {
         ArrayList a = new ArrayList();

         if (items != null)
         {
            foreach (ItemRow item in items)
            {
               // SSE data messages end with \n\n, so ensure that no such sequences
               // are embedded in the msg body
               string encodedDesc = null;

               if (!string.IsNullOrWhiteSpace(item.description))
                  encodedDesc = item.description.ReplaceNewlinesWithBreaks();

               // the client doesn't need to see all elements of an item,
               // so define a subset object
               dynamic d = new
               {
                  title = item.title.ReplaceNewlinesWithBreaks(),
                  url = item.url,
                  feedName = item.feedName,
                  pubDate = item.pubDate.ToString("u"),
                  insDate = item.insDate.ToString("u"),
                  description = encodedDesc
               };

               a.Add(d);
            }
         }

         string msg = "retry: " + (sleepSecs * 1000).ToString() +
            "\ndata: " + JsonConvert.SerializeObject(a) + "\nid: " + id + "\n\n";

         logger?.Debug("Sending: " + msg);

         return msg;
      }


      private string buildJsonResponse(Dictionary<string, int> stateTotals)
      {
         ArrayList a = new ArrayList();

         foreach (var s in stateTotals)
         {
            dynamic state = new { abbr = s.Key, total = s.Value };
            a.Add(state);
         }

         string msg = "retry: " + (sleepSecs * 1000).ToString() + "\ndata: " +
            JsonConvert.SerializeObject(a) + "\nid: |||ID|||\n\n";

         logger?.Debug("Sending: " + msg);

         return msg;
      }


      public bool IsReusable
      {
         get
         {
            return false;
         }
      }
   }
}