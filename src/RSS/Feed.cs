// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;


namespace RSS
{
   /// <summary>
   /// A Feed object represents the feed data located at a given URL.
   /// </summary>
   public class Feed : RSSBase
   {
      // feed content
      protected RSSDocument rssDoc;


      /// <summary>
      /// Downloads the latest copy of the feed from the publisher's site.
      /// </summary>
      public void refresh(string url)
      {
         RSSReader rr = new RSSReader(url);
         rssDoc = rr.read();
      }


      public IEnumerable<ParserError> errors()
      {
         if (rssDoc.channel.parseErrors != null  &&  rssDoc.channel.parseErrors.Count > 0)
            return (from e in rssDoc.channel.parseErrors select e.error);
         else
            return null;
      }
   }
}
