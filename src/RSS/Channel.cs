// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.Xml;


namespace RSS
{
   /// <summary>
   /// A channel contains header attributes describing the feed, and a list of
   /// child items.
   /// </summary>
   public class Channel : RSSBase
   {
      public string title;
      public string url;
      public string description;
      public DateTime pubDate;
      public DateTime lastBuildDate;
      public Item[] item;

      // some publishers don't include date elements, inject ad links, etc.,
      // so keep a list of invalid items. This will allow us to identify
      // specific problems and design workarounds if desired.
      public readonly List<RSSException> parseErrors = new List<RSSException>();

      
      public void read(XmlNode n)
      {
         title = selectString(n, "title", true);
         url = selectString(n, "link", false);

         if (string.IsNullOrEmpty(url))
            url = selectString(n, "origLink", false);

         description = selectString(n, "description", false);
         pubDate = selectDateTime(n, "pubDate");
         lastBuildDate = selectDateTime(n, "lastBuildDate");

         XmlNodeList xNodes = n.SelectNodes("item");

         if (xNodes == null)
            throw new RSSException(new ParserError("item", n.OuterXml,
               "Unable to locate item elements"));

         List<Item> items = new List<Item>();

         foreach (XmlNode xItem in xNodes)
         {
            try
            {
               items.Add(new Item(xItem));
            }
            catch (RSSException e)
            {
               parseErrors.Add(e);
            }
         }

         item = items.ToArray();
      }
   }
}
