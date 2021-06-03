// Copyright 2020 Richard Blasingame. All rights reserved.

using System.Xml;


namespace RSS
{
   /// <summary>
   /// This is the root-element representation of a RSS feed.
   /// </summary>
   public class RssDocument
   {
      public readonly decimal Version;
      public Channel Channel = new Channel();


      public RssDocument(string xml)
      {
         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(xml);
         XmlNode rss = xDoc.SelectSingleNode("//rss");

         string v = rss?.Attributes["version"]?.Value;

         if (!string.IsNullOrEmpty(v))
            decimal.TryParse(v, out Version);

         XmlNode n = rss?.SelectSingleNode("channel");

         Channel.Read(n);
      }
   }
}
