// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Xml;


namespace RSS
{
   public class RSSBase
   {
      protected string selectString(XmlNode n, string s, bool required)
      {
         XmlNode t = n.SelectSingleNode($"*[local-name()='{s}']");

         if (t != null)
            return t.InnerText;
         else if (!required)
            return null;
         else
            throw new RSSException(new ParserError(s, "", "Unable to locate required element"));
      }


      protected int selectInt(XmlNode n, string s)
      {
         XmlNode t = n.SelectSingleNode($"*[local-name()='{s}']");
         int i;

         if (t != null  &&  int.TryParse(t.InnerText, out i))
            return i;
         else
            throw new RSSException(new ParserError(t?.LocalName, s, "Unable to parse int"));
      }


      protected decimal selectDecimal(XmlNode n, string s)
      {
         XmlNode t = n.SelectSingleNode($"*[local-name()='{s}']");
         decimal d;

         if (t != null  &&  decimal.TryParse(t.InnerText, out d))
            return d;
         else
            throw new RSSException(new ParserError(t?.LocalName, s, "Unable to parse decimal"));
      }


      /// <summary>
      /// Some publishers don't adhere to ISO formatting, so we need to apply
      /// some tweaks to get them to parse properly.
      /// 
      /// ddd MMM dd HH:mm:ss Z yyyy     Wed Oct 07 08:00:07 GMT 2009
      /// dd MMM yyyy HH:mm:ss           06 Aug 2020 23:02:11
      /// dd MMM yyyy HH:mm:ss K         06 Aug 2020 23:02:11 +0000
      /// </summary>
      protected DateTime selectDateTime(XmlNode n, string s)
      {
         XmlNode t = n.SelectSingleNode($"*[local-name()='{s}']");

         if (t == null)
            return DateTime.MinValue.ToUniversalTime();

         string val = t.InnerText.Replace("UTC", "+0000").
            Replace("EST", "+0500").Replace("CST", "+0600").
            Replace("MST", "+0700").Replace("PST", "+0800").
            Replace("EDT", "+0400").Replace("CDT", "+0500").
            Replace("MDT", "+0600").Replace("PDT", "+0700");

         DateTime d;

         if (DateTime.TryParse(val, out d))
            return d.ToUniversalTime();
         else
            throw new RSSException(new ParserError(t?.LocalName, val, "Unable to parse date"));
      }
   }
}
