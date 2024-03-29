﻿using System;


namespace RSS
{
   /// <summary>
   /// A representation of a row in the rss_feed database table.
   /// </summary>
   public class FeedRow : Feed
   {
      // database columns
      public readonly string Title;
      public readonly string Url;
      public readonly int FeedId;
      public readonly bool IsRegional;
      public readonly bool IsActive;


      public FeedRow(int id, string t, string u, bool regional, bool act)
      {
         FeedId = id;
         Title = t;
         Url = u;
         IsRegional = regional;
         IsActive = act;
      }


      /// <summary>
      /// Save any new feed items to the database.
      /// </summary>
      public void Save(IDal dal)
      {
         if (RssDoc.Channel.Items == null)
            return;

         foreach (Item item in RssDoc.Channel.Items)
         {
            if (!dal.ItemExists(item.Url))
               dal.InsertItem(FeedId, item);
         }
      }
   }
}
