using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Util;


namespace UtilTests
{
   [TestClass]
   public class StringExtensionsTest
   {
      [TestMethod]
      public void RemoveAccentsTest()
      {
         Assert.AreEqual("Ooeaacz", "Óôéäãçž".RemoveAccents());
      }


      [TestMethod]
      public void ReplaceNewlinesWithBreaksTest()
      {
         Assert.AreEqual("<br/>This is a test<br/> string.<br/>Coda.",
            "\rThis is a test\n string.\r\nCoda.".ReplaceNewlinesWithBreaks());
      }


      [TestMethod("Validated by FormatXmlStringTest")]
      public void FormatXmlTest()
      {
      }


      [TestMethod]
      public void FormattedXmlTest()
      {
         string inXml = @"<rss><title>Top title</title><channel><item><title>Item title</title></item></channel></rss>";
         string outXml = inXml.FormattedXml();

         Assert.AreEqual(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<rss>
   <title>Top title</title>
   <channel>
      <item>
         <title>Item title</title>
      </item>
   </channel>
</rss>", outXml);
      }
   }
}
