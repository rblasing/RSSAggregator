// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Text.RegularExpressions;
using System.Xml;


namespace RSS
{
   /// <summary>
   /// An item represents a single article within a feed.
   /// </summary>
   public class Item : RSSBase
   {
      // RSS item elements
      public string title;
      public string description;
      public string url;
      public DateTime pubDate;

      // raw content, to be persisted in a database if desired
      public string itemXml;


      public Item()
      {
      }


      public Item(XmlNode n)
      {
         itemXml = n.OuterXml;
         title = selectString(n, "title", true);
         description = removeAds(selectString(n, "description", false));

         // the item link might be in any of the following elements
         url = selectString(n, "guid", false);

         if (string.IsNullOrWhiteSpace(url)  ||  !url.ToLower().StartsWith("http"))
            url = selectString(n, "origLink", false);

         if (string.IsNullOrWhiteSpace(url))
            url = selectString(n, "link", true);
         
         Uri u = new Uri(url);
         string hostAndPath = u.Host + u.AbsolutePath;

         // only check path, ignoring query params
         if (hostAndPath.Contains("rss")  ||  hostAndPath.Contains("feedburner")  ||
            hostAndPath.Contains("proxy"))
         {
            throw new RSSException(new ParserError("link", url, "Possible link redirection"));
         }

         if (url.Contains("contentmarketing")  ||  url.Contains("hppaidpartner")  ||
            url.Contains("cnn-underscored"))
         {
            throw new RSSException(new ParserError("item", n.OuterXml, "Possible advertisement"));
         }

         // probably an advertisement, so skip this item
         if (url.Length > 400)
            throw new RSSException(new ParserError("url", url, "Possible advertisement"));

         // the publication date might be in any of the following elements
         pubDate = selectDateTime(n, "pubDate");

         if (pubDate == DateTime.MinValue.ToUniversalTime())
            pubDate = selectDateTime(n, "date");

         if (pubDate == DateTime.MinValue.ToUniversalTime())
            pubDate = selectDateTime(n, "dateTimeWritten");

         if (pubDate == DateTime.MinValue.ToUniversalTime())
            pubDate = selectDateTime(n, "updateDate");

         if (pubDate == DateTime.MinValue.ToUniversalTime())
            throw new RSSException(new ParserError("pubDate", "", "Unable to locate pubDate"));
      }


      private string removeAds(string s)
      {
         if (string.IsNullOrWhiteSpace(s))
            return null;

         // remove all img tags
         string retVal = Regex.Replace(s, "<img.+?>", "", RegexOptions.Singleline);

         /*
          * Remove feedflare, ex:
          * 
          * <span class="itemdesc">If you're expecting the 2020 election to
          * result in another Bush v. Gore Supreme Court resolution -- or,
          * perhaps, in a nightmare scenario, multiples of that -- well, that
          * certainly could happen after November 3. Nobody wants a repeat of
          * the controversial 2000 Supreme Court decision that ended a
          * re-count of votes cast in Florida, effectively awarding Florida's
          * electoral votes, and the election as a whole, to George W. Bush.
          * <div class="feedflare"><br><br>
          * <a href="http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:yIl2AUoC8zA">
          * <img src="http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=yIl2AUoC8zA" border="0"></a>
          * <a href="http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:7Q72WNTAKBA">
          * <img src="http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=7Q72WNTAKBA" border="0"></a>
          * <a href="http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:V_sGLiPBpWU">
          * <img src="http://feeds.feedburner.com/~ff/rss/cnn_topstories?i=z0WNWlQE3Ck:uBEGyY3gYaI:V_sGLiPBpWU" border="0"></a>
          * <a href="http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:qj6IDK7rITs">
          * <img src="http://feeds.feedburner.com/~ff/rss/cnn_topstories?d=qj6IDK7rITs" border="0"></a>
          * <a href="http://rss.cnn.com/~ff/rss/cnn_topstories?a=z0WNWlQE3Ck:uBEGyY3gYaI:gIN9vFwOqvQ">
          * <img src="http://feeds.feedburner.com/~ff/rss/cnn_topstories?i=z0WNWlQE3Ck:uBEGyY3gYaI:gIN9vFwOqvQ" border="0"></a>
          * <br><br>
          * </div>
          * </span>
          */
         retVal = Regex.Replace(retVal,
            "<div class=\"feedflare\">.*?<\\/div>", "",
            RegexOptions.Singleline);

         retVal = Regex.Replace(retVal,
            "(<div(?:(?!<div).)*?Like on Facebook.*?<\\/div>)", "",
            RegexOptions.Singleline);

         return retVal;
      }


      public override bool Equals(object obj)
      {
         if (obj == null  ||  GetType() != obj.GetType())
            return false;

         Item that = (Item)obj;

         if (description != that.description)
            return false;

         if (itemXml != that.itemXml)
            return false;

         if (pubDate != that.pubDate)
            return false;

         if (title != that.title)
            return false;

         if (url != that.url)
            return false;

         return true;
      }


      public override int GetHashCode()
      {
         return base.GetHashCode();
      }
   }
}
