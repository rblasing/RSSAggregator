// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   public interface IDal
   {
      FeedRow[] GetFeeds();
      bool InsertFeed(string title, string url, bool isRegional);
      bool DeleteFeed(int feedId);
      bool EditFeed(int feedId, string title, string url, bool isRegional, bool isActive);
      bool InsertItem(int feedId, Item item);
      bool ItemExists(string url);
      ItemRow[] GetTopItems(int count);
      ItemRow[] GetNewItems(DateTime sinceDate);
      DateTime GetMaxInsertDate();
   }
}
