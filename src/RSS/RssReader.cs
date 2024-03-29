﻿using System;
using System.Net;


namespace RSS
{
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   class RssReader
   {
      private readonly string _url;


      public RssReader(string url) => this._url = url;


      /// <summary>
      /// Download XML for this feed and deserialize into a Channel object
      /// </summary>
      public RssDocument Read()
      {
         WebClient wc = null;

         try
         {
            wc = new WebClient() { Encoding = System.Text.Encoding.UTF8 };
            string xmlResponse = wc.DownloadString(_url);

            return new RssDocument(xmlResponse);
         }
         finally
         {
            wc?.Dispose();
         }
      }
   }
}
