using System;
using System.Collections.Generic;


namespace RSS
{
   /// <summary>
   /// A Feed object represents the feed resource located at a given URL.
   /// </summary>
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
