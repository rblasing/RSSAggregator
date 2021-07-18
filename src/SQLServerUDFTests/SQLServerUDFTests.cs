using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace SQLServerUDFTests
{
   [TestClass]
   public class SQLServerUDFTests
   {
      [TestMethod()]
      public void SplitStringTest()
      {
         string s = "This string has an article, and a conjunction, plus a preposition, and its pronoun is 'its', and it contains these common words: day, should, and big!";

         // all non-distinct words (single-letter words and numbers, ex: 'a', 'I', '3' are always excluded)
         // "This string has an article, and conjunction, plus preposition, and its pronoun is 'its', and it contains these common words: day, should, and big!";
         var words = SQLServerUDF.SQLServerUDF.SplitString(s, false, false, false, false, false);
         Assert.IsNotNull(words);
         Assert.AreEqual(24, words.Cast<string>().ToArray().Length);

         // all distinct words
         // "This string has an article, and conjunction, plus preposition, its pronoun is it contains these common words: day, should, big!"
         words = SQLServerUDF.SQLServerUDF.SplitString(s, false, false, false, false, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(20, words.Cast<string>().ToArray().Length);

         // minus pronouns
         // "string has an article, and conjunction, plus preposition, pronoun is contains common words: day, should, big!"
         words = SQLServerUDF.SQLServerUDF.SplitString(s, false, false, false, true, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(16, words.Cast<string>().ToArray().Length);

         // minus prepositions
         // "string has an article, and conjunction, preposition, pronoun is contains common words: day, should, big!"
         words = SQLServerUDF.SQLServerUDF.SplitString(s, false, false, true, true, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(15, words.Cast<string>().ToArray().Length);

         // minus conjunctions
         // "string has an article, conjunction, preposition, pronoun is contains common words: day, should, big!"
         words = SQLServerUDF.SQLServerUDF.SplitString(s, false, true, true, true, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(14, words.Cast<string>().ToArray().Length);

         // minus articles
         // "string has article, conjunction, preposition, pronoun is contains common words: day, should, big!"
         words = SQLServerUDF.SQLServerUDF.SplitString(s, true, true, true, true, true);
         Assert.IsNotNull(words);
         Assert.AreEqual(13, words.Cast<string>().ToArray().Length);
      }


      [TestMethod()]
      public void BleepTest()
      {
         string[] profanities = { "damn" };
         string s = SQLServerUDF.SQLServerUDF.Bleep(profanities, "This is a 'damn' test");
         Assert.AreEqual("This is a 'd--n' test", s);

         s = SQLServerUDF.SQLServerUDF.Bleep(profanities, "damn! This is a test");
         Assert.AreEqual("d--n! This is a test", s);

         s = SQLServerUDF.SQLServerUDF.Bleep(profanities, "Damn! This is a test");
         Assert.AreEqual("D--n! This is a test", s);

         s = SQLServerUDF.SQLServerUDF.Bleep(profanities, "Damn! This is a damnable test");
         Assert.AreEqual("D--n! This is a damnable test", s);

         s = SQLServerUDF.SQLServerUDF.Bleep(profanities, "My dear, I don't give a damn.");
         Assert.AreEqual("My dear, I don't give a d--n.", s);
      }


      [TestMethod()]
      public void NeedsBleepingTest()
      {
         string[] profanities = { "damn" };
         Assert.IsTrue(SQLServerUDF.SQLServerUDF.NeedsBleeping(profanities, "This is a 'damn' test"));
         Assert.IsTrue(SQLServerUDF.SQLServerUDF.NeedsBleeping(profanities, "This is a DAMN test"));
         Assert.IsFalse(SQLServerUDF.SQLServerUDF.NeedsBleeping(profanities, "This is a damnable test"));
      }
   }
}
