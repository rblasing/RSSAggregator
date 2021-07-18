using System;
using System.Xml;


namespace RSS
{
   public class RssBase
   {
      protected static string SelectString(XmlNode n, string s, bool required)
      {
         XmlNode t = n.SelectSingleNode($"*[local-name()='{s}']");

         if (t != null)
            return t.InnerText;
         else if (!required)
            return null;
         else
            throw new RssException(new ParserError(s, "", "Unable to locate required element"));
      }


      protected static int SelectInt(XmlNode n, string s)
      {
         XmlNode t = n.SelectSingleNode($"*[local-name()='{s}']");

         if (t != null  &&  int.TryParse(t.InnerText, out int i))
            return i;
         else
            throw new RssException(new ParserError(t?.LocalName, s, "Unable to parse int"));
      }


      protected static decimal SelectDecimal(XmlNode n, string s)
      {
         XmlNode t = n.SelectSingleNode($"*[local-name()='{s}']");

         if (t != null  &&  decimal.TryParse(t.InnerText, out decimal d))
            return d;
         else
            throw new RssException(new ParserError(t?.LocalName, s, "Unable to parse decimal"));
      }


      /// <summary>
      /// Some publishers don't adhere to ISO formatting, so we need to apply
      /// some tweaks to get them to parse properly.
      /// 
      /// ddd MMM dd HH:mm:ss Z yyyy     Wed Oct 07 08:00:07 GMT 2009
      /// dd MMM yyyy HH:mm:ss           06 Aug 2020 23:02:11
      /// dd MMM yyyy HH:mm:ss K         06 Aug 2020 23:02:11 +0000
      /// </summary>
      protected static DateTime SelectDateTime(XmlNode n, string s)
      {
         XmlNode t = n.SelectSingleNode($"*[local-name()='{s}']");

         if (t == null)
            return DateTime.MinValue.ToUniversalTime();

         string val = t.InnerText.Replace("UTC", "+0000").
            Replace("EST", "+0500").Replace("CST", "+0600").
            Replace("MST", "+0700").Replace("PST", "+0800").
            Replace("EDT", "+0400").Replace("CDT", "+0500").
            Replace("MDT", "+0600").Replace("PDT", "+0700");

         if (DateTime.TryParse(val, out DateTime d))
            return d.ToUniversalTime();
         else
            throw new RssException(new ParserError(t.LocalName, val, "Unable to parse date"));
      }
   }
}
