// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Xml;


namespace RSS
{
   public class RSSWriter
   {
      // data written to rollup feed
      private readonly SyndicationFeed feed = null;
      private List<SyndicationItem> items = null;
      private readonly string xsltUri;


      /// <summary>
      /// Create a <c>SyndicationFeed</c> object using the provided items.
      /// </summary>
      /// <param name="title">Title ot new feed</param>
      /// <param name="itemsToPublish">Set of items to be published</param>
      /// <param name="rssUri">URL where published feed may be found</param>
      /// <param name="xsltUri">URL where feed's linked XSL may be found</param>
      public RSSWriter(string title, Item[] itemsToPublish, string rssUri, string xsltUri)
      {
         if (itemsToPublish == null  ||  itemsToPublish.Length < 1)
            throw new InvalidOperationException("No items to write");

         foreach (Item item in itemsToPublish)
            addItem(item);

         this.xsltUri = xsltUri;

         feed = new SyndicationFeed();
         feed.LastUpdatedTime = DateTime.UtcNow;
         feed.Title = new TextSyndicationContent(title, TextSyndicationContentKind.Html);
         feed.BaseUri = new Uri(rssUri);
         feed.ElementExtensions.Add("ttl", "", "30");

         SyndicationLink link = new SyndicationLink(new Uri(rssUri));
         feed.Links.Add(link);

         feed.Items = items;
      }


      /// <summary>
      /// Add new feed item to list which is to be exported to rollup feed
      /// </summary>
      private void addItem(Item item)
      {
         if (items == null)
            items = new List<SyndicationItem>();

         SyndicationItem sItem = new SyndicationItem();
         sItem.Title = new TextSyndicationContent(item.title, TextSyndicationContentKind.Html);
         sItem.Summary = new TextSyndicationContent(item.description, TextSyndicationContentKind.Html);
         sItem.PublishDate = item.pubDate;
         SyndicationLink link = new SyndicationLink(new Uri(item.url));
         sItem.Links.Add(link);
         sItem.Id = item.url;
         sItem.AddPermalink(new Uri(item.url));
         items.Add(sItem);
      }


      /// <summary>
      /// Publish rollup feed items to a RSS file
      /// </summary>
      public void write(string filename)
      {
         Rss20FeedFormatter form = new Rss20FeedFormatter(feed);
         XmlTextWriter xw = null;

         try
         {
            xw = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
            xw.Formatting = Formatting.Indented;

            xw.WriteProcessingInstruction("xml-stylesheet",
               $"type='text/xsl' href='{xsltUri}'");

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
