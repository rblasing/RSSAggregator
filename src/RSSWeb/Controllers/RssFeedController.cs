// Copyright 2020 Richard Blasingame. All rights reserved.

using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

using RSSWeb.Models;
using RSS;
using Util;


namespace RSSWeb.Controllers
{
   [BasicAuthenticationAttribute("admin", "8675309", BasicRealm = "RSSWeb")]
   public class RssFeedController : Controller
   {
      private static readonly string ConnStr =
         ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;


      public ActionResult Index()
      {
         //select all feeds and return to view
         ModelState.Clear();
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            FeedRow[] feeds = dal.GetFeeds();
            List<RssFeedModel> mFeeds = new List<RssFeedModel>();

            foreach (FeedRow feed in feeds)
            {
               RssFeedModel mFeed = new RssFeedModel(feed.FeedId, feed.Title,
                  feed.Url, feed.IsRegional, feed.IsActive);

               mFeeds.Add(mFeed);
            }

            return View(mFeeds);
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


      public ActionResult Create() => View();


      [HttpPost]
      public ActionResult Create(RssFeedModel mFeed)
      {
         SqlConnection dbConn = null;

         try
         {
            if (ModelState.IsValid)
            {
               dbConn = new SqlConnection(ConnStr);
               dbConn.Open();
               Dal dal = new Dal(dbConn);

               if (dal.InsertFeed(mFeed.Title.RemoveAccents(), mFeed.Url, mFeed.Regional))
                  ViewBag.Message = "Success!";
               else
                  ViewBag.Message = "Unable to add new feed";

               ModelState.Clear();
            }

            return RedirectToAction("Index");
         }
         catch
         {
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


      public ActionResult Edit(int id)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            FeedRow[] feeds = dal.GetFeeds();
            FeedRow feed = feeds.First(f => f.FeedId == id);

            RssFeedModel mFeed = new RssFeedModel(
               feed.FeedId, feed.Title, feed.Url, feed.IsRegional, feed.IsActive);

            return View(mFeed);
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


      [HttpPost]
      public ActionResult Edit(int id, RssFeedModel mFeed)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            dal.EditFeed(id, mFeed.Title.RemoveAccents(), mFeed.Url, mFeed.Regional, mFeed.Active);

            return RedirectToAction("Index");
         }
         catch
         {
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


      public ActionResult Delete(int id)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            FeedRow[] feeds = dal.GetFeeds();
            FeedRow feed = feeds.First(f => f.FeedId == id);

            RssFeedModel mFeed = new RssFeedModel(
               feed.FeedId, feed.Title, feed.Url, feed.IsRegional, feed.IsActive);

            return View(mFeed);
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


      [HttpPost]
      public ActionResult Delete(int id, RssFeedModel mFeed)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            dal.DeleteFeed(id);
            ViewBag.Message = "Deleted";

            return RedirectToAction("Index");
         }
         catch
         {
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
   }
}