using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;


namespace RSS
{
   /// <summary>
   /// A channel contains header attributes describing the feed, and a list of
   /// child items which contain the actual articles.
   /// </summary>
   public class Channel : RssBase
   {
      public string Title { get; private set; }
      public string Url { get; private set; }
      public string Description { get; private set; }
      public DateTime PubDate { get; private set; }
      public DateTime LastBuildDate { get; private set; }

      private Item[] _item;

      public IEnumerable<Item> Items
      {
         get
         {
            if (_item == null  ||  _item.Length < 1)
               return null;
            else
               return _item.AsEnumerable();
         }
      }

      // some publishers don't include date elements, inject ad links, etc.,
      // so keep a list of invalid items. This will allow us to identify
      // specific problems and design workarounds if desired.
      private readonly List<RssException> _parseErrors = new List<RssException>();

      public IEnumerable<ParserError> Errors
      {
         get
         {
            if (_parseErrors != null  &&  _parseErrors.Count > 0)
               return (from e in _parseErrors select e.Error);
            else
               return null;
         }
      }


      public void Read(XmlNode n)
      {
         Title = SelectString(n, "title", true);
         Url = SelectString(n, "link", false);

         if (string.IsNullOrEmpty(Url))
            Url = SelectString(n, "origLink", false);

         Description = SelectString(n, "description", false);
         PubDate = SelectDateTime(n, "pubDate");
         LastBuildDate = SelectDateTime(n, "lastBuildDate");

         XmlNodeList xNodes = n.SelectNodes("item");

         if (xNodes == null)
            throw new RssException(new ParserError("item", n.OuterXml,
               "Unable to locate item elements"));

         List<Item> items = new List<Item>();

         foreach (XmlNode xItem in xNodes)
         {
            try
            {
               items.Add(new Item(xItem));
            }
            catch (RssException e)
            {
               _parseErrors.Add(e);
            }
         }

         _item = items.ToArray();
      }
   }
}
