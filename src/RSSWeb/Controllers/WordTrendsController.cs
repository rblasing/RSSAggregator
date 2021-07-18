using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

using Newtonsoft.Json;
using RSS;


namespace RSSWeb.Controllers
{
    public class WordTrendsController : ControllerBase
    {
      /// <summary>
      /// Get word/hour data for the initial view.
      /// </summary>
      public ActionResult Index()
      {
         ModelState.Clear();

         if (Session["hourRange"] == null)
            Session["hourRange"] = 24;

         if (Session["filter"] == null)
            Session["filter"] = "false";

         ViewBag.hourRange = Convert.ToInt32(Session["hourRange"]);
         ViewBag.filter = Convert.ToString(Session["filter"]);
         ViewBag.initData = GetWordsData(ViewBag.hourRange, ViewBag.filter == "true");

         return View();
      }


      /// <summary>
      /// If the user changes the hour range, the page will make an AJAX call to
      /// this method to return fresh data related to the new range.
      /// </summary>
      public ActionResult GetTrendingWords(int hours, bool filter)
      {
         Session["hourRange"] = hours;
         Session["filter"] = filter.ToString().ToLower();

         return Json(GetWordsData(hours, filter), JsonRequestBehavior.AllowGet);
      }


      private string GetWordsData(int hours, bool filter)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            DalQuery dal = new DalQuery(dbConn);
            IReadOnlyList<WordCount> words = dal.GetTrendingWords(hours, filter);

            System.Collections.ArrayList a = new System.Collections.ArrayList();

            if (words != null)
            {
               foreach (var word in words)
                  a.Add(new { word = word.Word, cnt = word.Count });
            }

            return JsonConvert.SerializeObject(a);
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
      /// When the user clicks on a graph bar, the page will make an AJAX call to
      /// this method to retrieve historial usage data for the selected word.
      /// </summary>
      public ActionResult GetWordHistory(string word, string hour)
      {
         SqlConnection dbConn = null;

         try
         {
            int hours = 24;
            int.TryParse(hour, out hours);

            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            DalQuery dal = new DalQuery(dbConn);
            IReadOnlyList<HourlyWordUsage> counts = dal.GetWordHistory(word, hours);

            List<dynamic> d = new List<dynamic>();

            if (counts != null)
            {
               foreach (var count in counts)
                  d.Add(new { hr = count.DateAndHour, cnt = count.Count });
            }

            return Json(JsonConvert.SerializeObject(d), JsonRequestBehavior.AllowGet);
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