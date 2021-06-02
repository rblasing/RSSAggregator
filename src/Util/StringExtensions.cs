using System;
using System.Linq;
using System.Text;
using System.Xml;


namespace Util
{
   public static class StringExtensions
   {
      /// <summary>
      /// Replaces accented characters (those with tildes, umlauts, etc)
      /// with an equivalent unaccented one.
      /// 
      /// Based on http://www.codeproject.com/Articles/13503/Stripping-Accents-from-Latin-Characters-A-Foray-in
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public static string RemoveAccents(this string s)
      {
         if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentNullException(nameof(s), "null s");

         StringBuilder sb = new StringBuilder();
         sb.Append(s.Normalize(NormalizationForm.FormKD).Where(x => x < 128).ToArray());

         return sb.ToString();
      }


      public static string ReplaceNewlinesWithBreaks(this string s)
      {
         if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentNullException(nameof(s), "null s");

         return s.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\r", "<br/>");
      }


      /// <summary>
      /// Applying the 'this' keyword to the first parameter causes this method
      /// to be treated as a class extension.
      /// </summary>
      public static string FormattedXml(this string s)
      {
         if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentNullException(nameof(s), "null s");

         XmlDocument xDoc = new XmlDocument();
         xDoc.LoadXml(s);

         return FormatXml(xDoc);
      }


      private static string FormatXml(XmlDocument doc)
      {
         if (doc == null)
            throw new ArgumentNullException(nameof(doc), "null doc");

         string ret;

         using (StringWriterWithEncoding swe =
            new StringWriterWithEncoding(new UTF8Encoding()))
         {

            XmlWriterSettings settings = new XmlWriterSettings()
            {
               Indent = true,
               IndentChars = "   ",
               NewLineChars = "\r\n",
               NewLineHandling = NewLineHandling.Replace
            };

            XmlWriter writer = XmlWriter.Create(swe, settings);
            doc.Save(writer);
            writer.Close();
            ret = swe.ToString();
         }

         return ret;
      }
   }
}
