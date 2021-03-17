// Copyright 2020 Richard Blasingame. All rights reserved.

using System;


namespace RSS
{
   public class ItemRow : Item
   {
      // table columns
      public int feedId;
      public string feedName;
      public DateTime insDate;


      public string toXml(bool isNew)
      {
         string className = isNew ? "newitem" : "olditem";

         return string.Format(
            "<item type='{0}'><url>{1}</url><title>{2}</title><pubDate>{3}</pubDate>" +
            "<insDate>{4}</insDate><desc>{5}</desc><feedName>{6}</feedName></item>",
            className,
            System.Web.HttpUtility.HtmlEncode(url),
            System.Web.HttpUtility.HtmlEncode(title),
            pubDate.ToString("u"),
            insDate.ToString("u"),
            System.Web.HttpUtility.HtmlEncode(description),
            feedName);
      }
   }
}
