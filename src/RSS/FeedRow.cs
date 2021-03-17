// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   public class FeedRow : Feed
   {
      // database columns
      public readonly string title;
      public readonly string url;
      public readonly int feedId;
      public readonly bool isRegional;
      public readonly bool isActive;


      public FeedRow(int id, string t, string u, bool regional, bool act)
      {
         feedId = id;
         title = t;
         url = u;
         isRegional = regional;
         isActive = act;
      }


      /// <summary>
      /// Save any new feed items to database.
      /// </summary>
      public void save(IDAL dal)
      {
         foreach (Item item in rssDoc.channel.item)
         {
            if (!dal.itemExists(item.url))
               dal.insertItem(feedId, item);
         }
      }
   }
}
