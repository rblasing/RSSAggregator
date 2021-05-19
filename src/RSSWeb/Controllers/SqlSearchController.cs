// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;

using RSS;
using Util;


namespace RSSWeb.Controllers
{
    public class SqlSearchController : Controller
    {
      private static readonly string ConnStr =
         ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;


      public ActionResult Index() => View();


      [HttpPost]
      public ActionResult Index(string Execute, string txtKeyword)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            DataTable dt = null;

            // execute a sample query based on the clicked button's 'value' attribute
            // (the clicked button's 'name' attribute is 'Execute')
            switch (Execute)
            {
               case "Media types":
                  dt = dal.Select("WITH t AS (SELECT xml.value('data(/item[1]/enclosure[1]/@type)', 'nvarchar(50)') AS type FROM rss_item WHERE xml.exist('/item/enclosure') = 1) " +
                     "SELECT t.type [Media Type], COUNT(*) [Count] FROM t GROUP BY t.type");

                  Session["dt"] = dt;
                  break;

               case "NASA references":
                  dt = dal.Select("SELECT title [Title] FROM rss_item WHERE xml.exist('/item/description/text()[contains(lower-case(.), \"nasa\")]') = 1");
                  Session["dt"] = dt;
                  break;

               case "Daily distribution":
                  dt = dal.Select("WITH t AS (SELECT xml.value('upper-case(substring(string((/item/pubDate)[1]), 1, 3))', 'nchar(3)') AS d FROM rss_item WHERE xml.exist('/item/pubDate') = 1) " +
                     "SELECT t.d [Day], COUNT(*) [Count] FROM t GROUP BY t.d ORDER BY COUNT(*) DESC");

                  Session["dt"] = dt;
                  break;

               case "Creators":
                  dt = dal.Select("SELECT DISTINCT CAST(xml.query('declare namespace dc=\"http://purl.org/dc/elements/1.1/\"; /item/dc:creator/text()') AS NVARCHAR(400)) [Creator] FROM rss_item WHERE " +
                     "xml.exist('declare namespace dc=\"http://purl.org/dc/elements/1.1/\"; /item/dc:creator') = 1 AND " +
                     "feed_id = (SELECT feed_id FROM rss_feed WHERE title = 'NPR All Things Considered') ORDER BY [Creator]");

                  Session["dt"] = dt;
                  break;

               case "Keyword search":
                  dt = dal.SelectByKeyword(txtKeyword);
                  Session["dt"] = dt;
                  break;

               case "Export":
                  dt = Session["dt"] as DataTable;
                  Export(dt);
                  dt = null;
                  break;
            }

            return View(dt);
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
      /// Send the search results back to the user client
      /// </summary>
      private void Export(DataTable dt)
      {
         MemoryStream ms = null;

         try
         {
            string[] columnNames = new string[dt.Columns.Count];

            for (int idx = 0; idx < columnNames.Length; idx++)
               columnNames[idx] = dt.Columns[idx].ColumnName;

            ms = new MemoryStream();
            OpenWorkbook.WriteOpenXmlWorkbook("", ms, dt.CreateDataReader(), columnNames);

            Response.Clear();

            Response.ContentType =
               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            Response.AddHeader("Content-Length", ms.Length.ToString());
            Response.AddHeader("Content-Disposition", "attachment; filename=SearchResults.xlsx");

            ms.Seek(0, SeekOrigin.Begin);
            ms.WriteTo(Response.OutputStream);
            ms.Close();
         }
         catch (Exception ex)
         {
            log4net.LogManager.GetLogger("RSSWeb").Error("Error exporting spreadsheet: " + ex.Message);
         }
         finally
         {
            if (ms != null)
            {
               ms.Close();
               ms.Dispose();
            }

            Response.Flush();
            Response.SuppressContent = true;
            HttpContext.ApplicationInstance.CompleteRequest();
         }
      }
   }
}