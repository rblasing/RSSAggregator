// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

using Newtonsoft.Json;
using RSS;


namespace Portfolio.Controllers
{
   public class StateMapController : Controller
   {
      private static readonly string sseUri = ConfigurationManager.AppSettings["sseUri"];
      private static readonly int heatMapHours = int.Parse((ConfigurationManager.AppSettings["heatMapHours"]));

      private static readonly string connStr =
         ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;


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
            dbConn = new SqlConnection(connStr);
            dbConn.Open();
            DAL dal = new DAL(dbConn);
            Dictionary<string, int> vals = dal.getStateActivityByRange(heatMapHours);

            System.Collections.ArrayList a = new System.Collections.ArrayList();

            foreach (var s in vals)
            {
               dynamic state = new { abbr = s.Key, total = s.Value };
               a.Add(state);
            }

            ViewBag.initData = JsonConvert.SerializeObject(a);
            ViewBag.sseUri = sseUri;

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
            dbConn = new SqlConnection(connStr);
            dbConn.Open();
            DAL dal = new DAL(dbConn);

            ItemRow[] rows = dal.getItemsByState(abbr, 10);

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