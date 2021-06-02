// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Activation;

using blasingame.RSS.xsd;
using RSS;


[assembly: log4net.Config.XmlConfigurator(Watch = true)]


namespace RSSWCFSvc
{
   [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
   [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
   public class RssService : blasingame.RSS.xsd.RSS
   {
      private static readonly string DbConnStr =
         ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;

      private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("RSSService");


      public getItemsResponse getItemsByKeyword(getItemsByKeywordRequest request)
      {
         SqlConnection dbConn = null;
         getItemsResponse rsp = new getItemsResponse() { Body = new getItemsResponseBody() };

         if (string.IsNullOrWhiteSpace(request.Body.keyword))
         {
            rsp.Body.error = new WSError(WSErrorType.InvalidArgument, false,
               "keyword", "keyword elememt is required");

            return rsp;
         }

         try
         {
            dbConn = new SqlConnection(DbConnStr);
            dbConn.Open();

            Dal dal = new Dal(dbConn);
            var items = dal.GetItemsByKeywords(request.Body.keyword);
            rsp.Body.items = BuildItemList(items);
         }
         catch (SqlException se)
         {
            rsp.Body.error = new WSError(WSErrorType.Database, true,
               se.ErrorCode.ToString(), se.Message);
         }
         catch (Exception e)
         {
            rsp.Body.error = new WSError(WSErrorType.Unknown, false,
               null, e.Message);

            Logger?.Error($"<error><message>{e.Message}</message><trace>{e.StackTrace}</trace></error>");
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

         return rsp;
      }


      public getItemsResponse getItemsByRange(getItemsByRangeRequest request)
      {
         SqlConnection dbConn = null;
         getItemsResponse rsp = new getItemsResponse() { Body = new getItemsResponseBody() };

         if (request.Body.minDateTime.Kind != DateTimeKind.Utc)
         {
            rsp.Body.error = new WSError(WSErrorType.InvalidArgument, false,
               "minDateTime", "minDateTime must be UTC");

            return rsp;
         }

         if (request.Body.maxDateTime.Kind != DateTimeKind.Utc)
         {
            rsp.Body.error = new WSError(WSErrorType.InvalidArgument, false,
               "maxDateTime", "maxDateTime must be UTC");

            return rsp;
         }

         if (request.Body.maxDateTime <= request.Body.minDateTime)
         {
            rsp.Body.error = new WSError(WSErrorType.InvalidArgument, false,
               "", "minDateTime must be less than maxDateTime");

            return rsp;
         }

         try
         {
            dbConn = new SqlConnection(DbConnStr);
            dbConn.Open();

            Dal dal = new Dal(dbConn);

            var items = dal.GetItemsByRange(request.Body.minDateTime,
               request.Body.maxDateTime);

            rsp.Body.items = BuildItemList(items);
         }
         catch (SqlException se)
         {
            rsp.Body.error = new WSError(WSErrorType.Database, true,
               se.ErrorCode.ToString(), se.Message);
         }
         catch (Exception e)
         {
            rsp.Body.error = new WSError(WSErrorType.Unknown, false,
               null, e.Message);

            Logger?.Error($"<error><message>{e.Message}</message><trace>{e.StackTrace}</trace></error>");
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

         return rsp;
      }


      public getItemsResponse getTopItems(getTopItemsRequest request)
      {
         SqlConnection dbConn = null;
         getItemsResponse rsp = new getItemsResponse() { Body = new getItemsResponseBody() };

         if (request.Body.itemCount <= 0)
         {
            rsp.Body.error = new WSError(WSErrorType.InvalidArgument, false,
               "itemCount", "itemCount must be greater than zero");

            return rsp;
         }

         try
         {
            dbConn = new SqlConnection(DbConnStr);
            dbConn.Open();

            Dal dal = new Dal(dbConn);
            var items = dal.GetTopItems((int)request.Body.itemCount);
            rsp.Body.items = BuildItemList(items);
         }
         catch (SqlException se)
         {
            rsp.Body.error = new WSError(WSErrorType.Database, true,
               se.ErrorCode.ToString(), se.Message);
         }
         catch (Exception e)
         {
            rsp.Body.error = new WSError(WSErrorType.Unknown, false,
               null, e.Message);

            Logger?.Error($"<error><message>{e.Message}</message><trace>{e.StackTrace}</trace></error>");
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

         return rsp;
      }


      private static ItemList BuildItemList(IReadOnlyList<ItemRow> items)
      {
         if (items == null)
            return null;

         ItemList list = new ItemList();

         foreach (ItemRow item in items)
         {
            blasingame.RSS.xsd.Item i = new blasingame.RSS.xsd.Item()
            {
               description = item.Description,
               link = new Uri(item.Url),
               pubDate = item.PubDate,
               publisher = item.FeedName,
               title = item.Title
            };

            list.Add(i);
         }

         return (list.Count > 0 ? list : null);
      }
   }
}