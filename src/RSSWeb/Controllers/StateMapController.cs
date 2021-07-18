using System;
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
            DalQuery dal = new DalQuery(dbConn);
            IReadOnlyList<StateActivity> vals = dal.GetStateActivityByRange(HeatMapHours);

            System.Collections.ArrayList a = new System.Collections.ArrayList();

            if (vals != null)
            {
               foreach (var s in vals)
               {
                  dynamic state = new { abbr = s.StateAbbr, total = s.Count };
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
            DalQuery dal = new DalQuery(dbConn);

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



      /// <summary>
      /// When the user clicks on the 'Replay' icon, the page will make an AJAX
      /// call to this method to retrieve a set of state activity snapshots over
      /// the previous 48 hours.
      /// </summary>
      public ActionResult GetStateHistory()
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            DalQuery dal = new DalQuery(dbConn);
            var sets = dal.GetStateActivityHistory(10, 48);

            System.Collections.ArrayList aSets = new System.Collections.ArrayList();

            if (sets != null)
            {
               foreach (var set in sets)
               {
                  System.Collections.ArrayList a = new System.Collections.ArrayList();

                  foreach (var v in set.Value)
                  {
                     dynamic state = new { abbr = v.StateAbbr, total = v.Count };
                     a.Add(state);
                  }

                  aSets.Add(a);
               }
            }

            return Json(JsonConvert.SerializeObject(aSets), JsonRequestBehavior.AllowGet);
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