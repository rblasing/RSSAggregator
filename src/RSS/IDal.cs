// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;


namespace RSS
{
   public interface IDal
   {
      IReadOnlyList<FeedRow> GetFeeds();
      bool InsertFeed(string title, string url, bool isRegional);
      bool DeleteFeed(int feedId);
      bool EditFeed(int feedId, string title, string url, bool isRegional, bool isActive);
      bool InsertItem(int feedId, Item item);
      bool ItemExists(string url);
      IReadOnlyList<ItemRow> GetTopItems(int count);
      IReadOnlyList<ItemRow> GetNewItems(DateTime sinceDate);
      DateTime GetMaxInsertDate();
   }
}
