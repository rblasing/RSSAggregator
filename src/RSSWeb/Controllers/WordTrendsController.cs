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

         ViewBag.hourRange = Convert.ToInt32(Session["hourRange"]);
         ViewBag.initData = GetWordsData(ViewBag.hourRange);

         return View();
      }


      /// <summary>
      /// If the user changes the hour range, the page will make an AJAX call to
      /// this method to return fresh data related to the new range.
      /// </summary>
      public ActionResult GetTrendingWords(int hours)
      {
         Session["hourRange"] = hours;

         return Json(GetWordsData(hours), JsonRequestBehavior.AllowGet);
      }


      private string GetWordsData(int hours)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            Dictionary<string, int> words = dal.GetTrendingWords(hours);

            System.Collections.ArrayList a = new System.Collections.ArrayList();

            foreach (var word in words)
               a.Add(new { word = word.Key, cnt = word.Value });

            string json = JsonConvert.SerializeObject(a);

            return json;
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
            Dal dal = new Dal(dbConn);
            Dictionary<string, int> counts = dal.GetWordHistory(word, hours);

            List<dynamic> d = new List<dynamic>();

            foreach (var count in counts)
               d.Add(new { hr = count.Key, cnt = count.Value });

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