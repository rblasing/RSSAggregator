// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Xml;


namespace RSS
{
   public class RssWriter
   {
      // data written to rollup feed
      private readonly SyndicationFeed _feed;
      private List<SyndicationItem> _items;
      private readonly string _xsltUri;


      /// <summary>
      /// Create a <c>SyndicationFeed</c> object using the provided items.
      /// </summary>
      /// <param name="title">Title ot new feed</param>
      /// <param name="itemsToPublish">Set of items to be published</param>
      /// <param name="rssUri">URL where published feed may be found</param>
      /// <param name="xsltUri">URL where feed's linked XSL may be found</param>
      public RssWriter(string title, Item[] itemsToPublish, string rssUri, string xsltUri)
      {
         if (itemsToPublish == null  ||  itemsToPublish.Length < 1)
            throw new InvalidOperationException("No items to write");

         foreach (Item item in itemsToPublish)
            AddItem(item);

         this._xsltUri = xsltUri;

         _feed = new SyndicationFeed()
         {
            LastUpdatedTime = DateTime.UtcNow,
            Title = new TextSyndicationContent(title, TextSyndicationContentKind.Html),
            BaseUri = new Uri(rssUri),
         };

         _feed.ElementExtensions.Add("ttl", "", "30");

         SyndicationLink link = new SyndicationLink(new Uri(rssUri));
         _feed.Links.Add(link);

         _feed.Items = _items;
      }


      /// <summary>
      /// Add new feed item to list which is to be exported to rollup feed
      /// </summary>
      private void AddItem(Item item)
      {
         if (_items == null)
            _items = new List<SyndicationItem>();

         SyndicationItem sItem = new SyndicationItem()
         {
            Title = new TextSyndicationContent(item.Title, TextSyndicationContentKind.Html),
            Summary = new TextSyndicationContent(item.Description, TextSyndicationContentKind.Html),
            PublishDate = item.PubDate
         };
   
         SyndicationLink link = new SyndicationLink(new Uri(item.Url));
         sItem.Links.Add(link);
         sItem.Id = item.Url;
         sItem.AddPermalink(new Uri(item.Url));
         _items.Add(sItem);
      }


      /// <summary>
      /// Publish rollup feed items to a RSS file
      /// </summary>
      public void Write(string filename)
      {
         Rss20FeedFormatter form = new Rss20FeedFormatter(_feed);
         XmlTextWriter xw = null;

         try
         {
            xw = new XmlTextWriter(filename, System.Text.Encoding.UTF8)
            {
               Formatting = Formatting.Indented
            };

            xw.WriteProcessingInstruction("xml-stylesheet",
               $"type='text/xsl' href='{_xsltUri}'");

            form.WriteTo(xw);
            xw.Close();
         }
         finally
         {
            xw?.Dispose();
         }
      }
   }
}
