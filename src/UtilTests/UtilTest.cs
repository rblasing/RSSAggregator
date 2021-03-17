// Copyright 2020 Richard Blasingame.All rights reserved.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Util;


namespace UtilTests
{
   [TestClass]
   public class UtilTest
   {
      [TestMethod]
      public void RemoveAccentsTest()
      {
         Assert.AreEqual("Ooeaacz", "Óôéäãçž".RemoveAccents());
      }


      [TestMethod("Validated by FormatXmlStringTest")]
      public void FormatXmlTest()
      {
      }


      [TestMethod]
      public void FormatXmlStringTest()
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
