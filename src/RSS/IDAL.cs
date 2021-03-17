// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   public interface IDAL
   {
      FeedRow[] getFeeds();
      bool insertFeed(string title, string url, bool isRegional);
      bool deleteFeed(int feedId);
      bool editFeed(int feedId, string title, string url, bool isRegional, bool isActive);
      bool insertItem(int feedId, Item item);
      bool itemExists(string url);
      ItemRow[] getTopItems(int count);
      ItemRow[] getNewItems(DateTime sinceDate);
      DateTime getMaxInsertDate();
   }
}
