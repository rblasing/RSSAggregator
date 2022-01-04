using System;


namespace RSS
{
   /// <summary>
   /// Represents a row in the rss_item database table.
   /// </summary>
   public class ItemRow : Item
   {
      // table columns associated with a feed item, but not a part of the
      // RSS XML schema
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
            PubDate.ToString("o"),
            InsDate.ToString("o"),
            System.Web.HttpUtility.HtmlEncode(Description),
            FeedName);
      }
   }
}
