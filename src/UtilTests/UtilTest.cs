// Copyright 2020 Richard Blasingame.All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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


      [TestMethod()]
      public void SplitStringTest()
      {
         string s = "This string has an article, and a conjunction, plus a preposition, and its pronoun is 'its', and it contains these common words: day, should, and big!";

         // all non-distinct words (single-letter words and numbers, ex: 'a', 'I', '3' are always excluded)
         // "This string has an article, and conjunction, plus preposition, and its pronoun is 'its', and it contains these common words: day, should, and big!";
         var words = SQLServerStringUtils.SplitString(s, false, false, false, false, false);
         Assert.IsNotNull(words);
         Assert.AreEqual(24, words.Cast<string>().ToArray().Length);

         // all distinct words
         // "This string has an article, and conjunction, plus preposition, its pronoun is it contains these common words: day, should, big!"
         words = SQLServerStringUtils.SplitString(s, false, false, false, false, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(20, words.Cast<string>().ToArray().Length);

         // minus pronouns
         // "string has an article, and conjunction, plus preposition, pronoun is contains common words: day, should, big!"
         words = SQLServerStringUtils.SplitString(s, false, false, false, true, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(16, words.Cast<string>().ToArray().Length);

         // minus prepositions
         // "string has an article, and conjunction, preposition, pronoun is contains common words: day, should, big!"
         words = SQLServerStringUtils.SplitString(s, false, false, true, true, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(15, words.Cast<string>().ToArray().Length);

         // minus conjunctions
         // "string has an article, conjunction, preposition, pronoun is contains common words: day, should, big!"
         words = SQLServerStringUtils.SplitString(s, false, true, true, true, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(14, words.Cast<string>().ToArray().Length);

         // minus articles
         // "string has article, conjunction, preposition, pronoun is contains common words: day, should, big!"
         words = SQLServerStringUtils.SplitString(s, true, true, true, true, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(13, words.Cast<string>().ToArray().Length);
      }
   }
}
