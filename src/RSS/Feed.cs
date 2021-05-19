// Copyright 2020 Richard Blasingame. All rights reserved.

using System.Collections.Generic;


namespace RSS
{
   /// <summary>
   /// A Feed object represents the feed data located at a given URL.
   /// </summary>
   public class Feed : RssBase
   {
      // feed content
      protected RssDocument RssDoc;


      /// <summary>
      /// Downloads the latest copy of the feed from the publisher's site.
      /// </summary>
      public void Refresh(string url)
      {
         RssReader rr = new RssReader(url);
         RssDoc = rr.Read();
      }


      public IEnumerable<ParserError> Errors() => RssDoc.Channel.Errors;
   }
}
