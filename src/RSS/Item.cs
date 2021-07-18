using System;
using System.Text.RegularExpressions;
using System.Xml;


namespace RSS
{
   /// <summary>
   /// An item represents a single article within a feed.
   /// </summary>
   public class Item : RssBase
   {
      // RSS item elements
      public string Title;
      public string Description;
      public string Url;
      public DateTime PubDate;

      // raw content, to be persisted in a database if desired
      public string ItemXml;


      public Item()
      {
      }


      public Item(XmlNode n)
      {
         ItemXml = n.OuterXml;
         Title = SelectString(n, "title", true);
         Description = RemoveAds(SelectString(n, "description", false));

         // don't persist invalidly-formed HTML fragments
         try
         {
            if (!string.IsNullOrWhiteSpace(Description))
            {
               XmlDocument xDoc = new XmlDocument();
               xDoc.LoadXml($"<doc>{Description}</doc>");
            }
         }
         catch (XmlException)
         {
            Description = string.Empty;
         }

         // the item link might be in any of the following elements
         Url = SelectString(n, "guid", false);

         if (string.IsNullOrWhiteSpace(Url)  ||  !Url.ToLower().StartsWith("http"))
            Url = SelectString(n, "origLink", false);

         if (string.IsNullOrWhiteSpace(Url))
            Url = SelectString(n, "link", true);
         
         Uri u = new Uri(Url);
         string hostAndPath = u.Host + u.AbsolutePath;

         // only check path, ignoring query params
         if (hostAndPath.Contains("rss")  ||  hostAndPath.Contains("feedburner")  ||
            hostAndPath.Contains("proxy"))
         {
            throw new RssException(new ParserError("link", Url, "Possible link redirection"));
         }

         if (Url.Contains("contentmarketing")  ||  Url.Contains("hppaidpartner")  ||
            Url.Contains("cnn-underscored"))
         {
            throw new RssException(new ParserError("item", n.OuterXml, "Possible advertisement"));
         }

         // probably an advertisement, so skip this item
         if (Url.Length > 400)
            throw new RssException(new ParserError("url", Url, "Possible advertisement"));

         // the publication date might be in any of the following elements
         PubDate = SelectDateTime(n, "pubDate");

         if (PubDate == DateTime.MinValue.ToUniversalTime())
            PubDate = SelectDateTime(n, "date");

         if (PubDate == DateTime.MinValue.ToUniversalTime())
            PubDate = SelectDateTime(n, "dateTimeWritten");

         if (PubDate == DateTime.MinValue.ToUniversalTime())
            PubDate = SelectDateTime(n, "updateDate");

         if (PubDate == DateTime.MinValue.ToUniversalTime())
            throw new RssException(new ParserError("pubDate", "", "Unable to locate pubDate"));
      }


      public string DescriptionInnerText()
      {
         if (string.IsNullOrWhiteSpace(Description))
            return string.Empty;

         XmlDocument x = new XmlDocument();
         x.LoadXml($"<doc>{Description.Replace("&", "&amp;")}</doc>");

         return x.InnerText;
      }


      private static string RemoveAds(string s)
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

         if (Description != that.Description)
            return false;

         if (ItemXml != that.ItemXml)
            return false;

         if (PubDate != that.PubDate)
            return false;

         if (Title != that.Title)
            return false;

         if (Url != that.Url)
            return false;

         return true;
      }


      [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      public override int GetHashCode() => base.GetHashCode();
   }
}
