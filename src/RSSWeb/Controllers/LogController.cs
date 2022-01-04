using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

using Util;


namespace RSSWeb.Controllers
{
   /// <summary>
   /// Paged view of the log4net table.
   /// </summary>
   public class LogController : ControllerBase
   {
      public ActionResult Index()
      {
         return View(GetPage(0));
      }


      public ActionResult Next(int id)
      {
         return View("Index", GetPage(id));
      }


      public ActionResult Previous(int id)
      {
         return View("Index", GetPage(id));
      }


      private Models.LogModel GetPage(int id)
      {
         if (id < 0)
            id = 0;

         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();

            TablePager tp = new TablePager(dbConn,
               "SELECT COUNT(*) FROM log4net",
               "SELECT id, date, thread, level, logger, message, exception FROM log4net ORDER BY date DESC", 50);

            DataTable dt = tp.GetPage(dbConn, id);

            Models.LogModel model = new Models.LogModel();
            model.PageNumber = id;
            model.PageCount = tp.NumPages;

            if (dt != null  &&  dt.Rows.Count > 0)
               model.LogEntries = new System.Collections.Generic.List<RSS.LogEntry>();

            foreach (DataRow row in dt.Rows)
            {
               RSS.LogEntry entry = new RSS.LogEntry();
               entry.id = (int)(row["id"]);
               entry.date = DateTime.SpecifyKind(Convert.ToDateTime(row["date"]), DateTimeKind.Utc);
               entry.exception = (string)(row["exception"]);
               entry.level = (string)(row["level"]);
               entry.logger = (string)(row["logger"]);
               entry.message = (string)(row["message"]);
               entry.thread = (string)(row["thread"]);
               model.LogEntries.Add(entry);
            }

            return model;
         }
         finally
         {
            if (dbConn != null)
            {
               if (dbConn.State != ConnectionState.Closed)
                  dbConn.Close();

               dbConn.Dispose();
            }
         }
      }
   }
}