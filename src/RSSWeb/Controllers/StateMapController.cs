// Copyright 2020 Richard Blasingame. All rights reserved.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

using Newtonsoft.Json;
using RSS;


namespace RSSWeb.Controllers
{
   public class StateMapController : ControllerBase
   {
      /// <summary>
      /// Retrieve a collection of statistics to use in the initial view of this
      /// page.
      /// </summary>
      public ActionResult Index()
      {
         ModelState.Clear();
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            Dictionary<string, int> vals = dal.GetStateActivityByRange(HeatMapHours);

            System.Collections.ArrayList a = new System.Collections.ArrayList();

            if (vals != null)
            {
               foreach (var s in vals)
               {
                  dynamic state = new { abbr = s.Key, total = s.Value };
                  a.Add(state);
               }
            }

            ViewBag.initData = JsonConvert.SerializeObject(a);
            ViewBag.sseUri = SseUri;

            return View();
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
      /// When the user clicks on a state, the page will make an AJAX call to
      /// this method to retrieve a set of recent items related to the clicked
      /// state.
      /// </summary>
      public ActionResult GetStateItems(string abbr)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);

            var rows = dal.GetItemsByState(abbr, 10);

            return Json(JsonConvert.SerializeObject(rows), JsonRequestBehavior.AllowGet);
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