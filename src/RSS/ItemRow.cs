// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   public class ItemRow : Item
   {
      // table columns
      public int FeedId;
      public string FeedName;
      public DateTime InsDate;


      public string ToXml(bool isNew)
      {
         string className = isNew ? "newitem" : "olditem";

         return string.Format(
            "<item type='{0}'><url>{1}</url><title>{2}</title><pubDate>{3}</pubDate>" +
            "<insDate>{4}</insDate><desc>{5}</desc><feedName>{6}</feedName></item>",
            className,
            System.Web.HttpUtility.HtmlEncode(Url),
            System.Web.HttpUtility.HtmlEncode(Title),
            PubDate.ToString("u"),
            InsDate.ToString("u"),
            System.Web.HttpUtility.HtmlEncode(Description),
            FeedName);
      }
   }
}
