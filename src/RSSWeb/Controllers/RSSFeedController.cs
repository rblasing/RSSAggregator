﻿// Copyright 2020 Richard Blasingame. All rights reserved.

using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

using Portfolio.Models;
using RSS;
using Util;


namespace Portfolio.Controllers
{
   [BasicAuthenticationAttribute("admin", "8675309", BasicRealm = "Portfolio")]
   public class RSSFeedController : Controller
   {
      private static readonly string connStr =
         ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;


      public ActionResult Index()
      {
         //select all feeds and return to view
         ModelState.Clear();
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(connStr);
            dbConn.Open();
            DAL dal = new DAL(dbConn);
            FeedRow[] feeds = dal.getFeeds();
            List<RSSFeedModel> mFeeds = new List<RSSFeedModel>();

            foreach (FeedRow feed in feeds)
            {
               RSSFeedModel mFeed = new RSSFeedModel(feed.feedId, feed.title,
                  feed.url, feed.isRegional, feed.isActive);

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


      public ActionResult Create()
      {
         return View();
      }


      [HttpPost]
      public ActionResult Create(RSSFeedModel mFeed)
      {
         SqlConnection dbConn = null;

         try
         {
            if (ModelState.IsValid)
            {
               dbConn = new SqlConnection(connStr);
               dbConn.Open();
               DAL dal = new DAL(dbConn);

               if (dal.insertFeed(mFeed.title.RemoveAccents(), mFeed.url, mFeed.regional))
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
            dbConn = new SqlConnection(connStr);
            dbConn.Open();
            DAL dal = new DAL(dbConn);
            FeedRow[] feeds = dal.getFeeds();
            FeedRow feed = feeds.First(f => f.feedId == id);

            RSSFeedModel mFeed = new RSSFeedModel(
               feed.feedId, feed.title, feed.url, feed.isRegional, feed.isActive);

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
      public ActionResult Edit(int id, RSSFeedModel mFeed)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(connStr);
            dbConn.Open();
            DAL dal = new DAL(dbConn);
            dal.editFeed(id, mFeed.title.RemoveAccents(), mFeed.url, mFeed.regional, mFeed.active);

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
            dbConn = new SqlConnection(connStr);
            dbConn.Open();
            DAL dal = new DAL(dbConn);
            FeedRow[] feeds = dal.getFeeds();
            FeedRow feed = feeds.First(f => f.feedId == id);

            RSSFeedModel mFeed = new RSSFeedModel(
               feed.feedId, feed.title, feed.url, feed.isRegional, feed.isActive);

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
      public ActionResult Delete(int id, RSSFeedModel mFeed)
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(connStr);
            dbConn.Open();
            DAL dal = new DAL(dbConn);
            dal.deleteFeed(id);
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