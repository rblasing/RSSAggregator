// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Xml;


namespace RSS
{
   /// <summary>
   /// This is the root-element representation of a RSS feed.
   /// </summary>
   public class RSSDocument
   {
      public readonly decimal version;
      public readonly Channel channel = new Channel();


      public RSSDocument(string xml)
      {
         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(xml);
         XmlNode rss = xDoc.SelectSingleNode("//rss");

         string v = rss.Attributes["version"].Value;

         if (!string.IsNullOrEmpty(v))
            decimal.TryParse(v, out version);

         XmlNode n = rss.SelectSingleNode("channel");

         channel.read(n);
      }
   }
}
