// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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
                  dt = dal.SelectExec("SelectMIMETypes");
                  Session["dt"] = dt;
                  break;

               case "NASA references":
                  dt = dal.SelectExec("SelectNASAArticles");
                  Session["dt"] = dt;
                  break;

               case "Daily distribution":
                  dt = dal.SelectExec("SelectDailyDistribution");
                  Session["dt"] = dt;
                  break;

               case "Creators":
                  dt = dal.SelectExec("SelectNPRAuthors");
                  Session["dt"] = dt;
                  break;

               case "Keyword search":
                  dt = dal.SelectByKeyword(txtKeyword);
                  Session["dt"] = dt;
                  break;

               case "Trending keywords":
                  try
                  {
                     dt = dal.GetTrendingWords(24);
                  }
                  // GetTrendingWords isn't supported by the current SQL Server
                  // environment, so execute equivalent fallback code
                  catch (InvalidOperationException)
                  {
                     ItemRow[] items = dal.GetItemsByRange(
                        DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

                     var splits = items.Select(
                        i => SQLServerStringUtils.SplitString(
                           i.Title.ToLower() + " " + i.DescriptionInnerText().ToLower(),
                           true, true, true, true, true, true));

                     var words = splits.SelectMany(s => s.Cast<string>().ToArray())
                        .GroupBy(g => g)
                        .Select(o => new { word = o.Key, count = o.Count() })
                        .OrderByDescending(d => d.count)
                        .Take(100);

                     dt = new DataTable();
                     dt.Columns.Add("Keyword", typeof(string));
                     dt.Columns.Add("Count", typeof(int));

                     foreach (var word in words)
                     {
                        DataRow r = dt.NewRow();
                        r["Keyword"] = word.word;
                        r["Count"] = word.count;
                        dt.Rows.Add(r);
                     }
                  }
                  
                  Session["dt"] = dt;
                  break;

               case "Export":
                  dt = Session["dt"] as DataTable;

                  if (dt != null)
                     Export(dt);
                  else
                     ViewBag.Error = "Session timed out.  Please refresh page and retry export";
   
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