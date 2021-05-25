// Copyright 2020 Richard Blasingame. All rights reserved.

using System.Net;


namespace RSS
{
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
