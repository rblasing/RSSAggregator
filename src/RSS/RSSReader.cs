// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.IO;
using System.Net;


namespace RSS
{
   class RSSReader
   {
      private readonly string url;


      public RSSReader(string url)
      {
         this.url = url;
      }
      
      
      /// <summary>
      /// Download XML for this feed and deserialize into a Channel object
      /// </summary>
      public RSSDocument read()
      {
         WebClient wc = new WebClient();
         wc.Encoding = System.Text.Encoding.UTF8;
         string xmlResponse = wc.DownloadString(url);

         return new RSSDocument(xmlResponse);
      }
   }
}
