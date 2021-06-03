using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.SqlServer.Server;


namespace Util
{
   /// <summary>
   /// If supported by the target instance of SQL Server, this class may be
   /// compiled as a separate library and installed as a CLR assembly of
   /// user-defined functions using the following instructions.

   /// To compile:
   /// 
   /// c:\windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:library SQLServerStringUtils.cs
   /// 
   /// Before initial installation:
   /// 
   /// sp_configure 'clr enabled', 1
   /// go
   /// RECONFIGURE
   /// go
   /// sp_configure 'show advanced options', 1
   /// go
   /// RECONFIGURE
   /// go
   /// sp_configure 'clr strict security', 0
   /// go
   /// RECONFIGURE
   /// go
   ///
   /// To install or refresh:
   /// 
   /// DROP FUNCTION dbo.UdfSplitString;
   /// go
   /// DROP ASSEMBLY StringUtils;
   /// go
   /// CREATE ASSEMBLY StringUtils 
   ///    FROM 'c:\data\source\Portfolio\Util\SQLServerStringUtils.dll' WITH PERMISSION_SET = SAFE;
   /// go
   /// CREATE FUNCTION UdfSplitString(@s NVARCHAR(MAX), @excludeArticles BIT, @excludeConjunctions BIT, @excludePrepositions BIT, @excludePronouns BIT, @distinctOnly BIT) 
   ///    RETURNS TABLE(word NVARCHAR(MAX)) AS EXTERNAL NAME StringUtils.[Util.SQLServerStringUtils].UdfSplitString;
   /// go
   /// 
   /// Usage (find top 100 most frequently-used words in recent items):
   /// 
   /// SELECT TOP 100 CAST(word AS NVARCHAR(60)), COUNT(*) FROM rss_item AS i 
   ///    CROSS APPLY dbo.UdfSplitString(LOWER(i.title) + ' ' + LOWER(COALESCE(i.description, '')), 1, 1, 1, 1, 1) WHERE
   ///    i.ins_date > '2021-5-20' GROUP BY word ORDER BY COUNT(*) DESC;
   /// </summary>
   public static class SQLServerStringUtils
   {
      readonly static string[] Articles = { "a", "an", "the" };

      readonly static string[] Conjunctions = { "and", "but", "for", "nor", "or", "so", "yet" };

      readonly static string[] Prepositions =
      {
         "about", "above", "across", "after", "against", "ago", "ahead", "along",
         "alongside", "amid", "among", "as", "around", "astride", "at", "atop",
         "away", "bar", "barring", "before", "behind", "below", "beneath",
         "beside", "besides", "between", "beyond", "but", "by", "circa",
         "concerning", "considering", "counting", "despite", "down", "during",
         "except", "excepting", "excluding", "far", "following", "for", "from",
         "given", "gone", "in", "including", "inside", "into", "less", "like",
         "minus", "near", "notwithstanding", "of", "off", "on", "onto", "opposite",
         "out", "outside", "over", "past", "pending", "per", "plus", "pro", "re",
         "regarding", "round", "save", "saving", "since", "than", "through",
         "throughout", "to", "toward", "towards", "under", "underneath", "unlike",
         "up", "upon", "versus", "via", "with", "within", "without", "worth"
      };

      readonly static string[] Pronouns =
      {
         "all", "another", "any", "anybody", "anyone", "anything", "as", "aught",
         "both", "each", "either", "enough", "everybody", "everyone", "everything",
         "few", "he", "her", "hers", "herself", "him", "himself", "his", "i",
         "idem", "it", "its", "itself", "many", "me", "mine", "most", "my",
         "myself", "naught", "neither", "nobody", "none", "nothing", "nought",
         "one", "other", "others", "ought", "our", "ours", "ourself", "ourselves",
         "several", "she", "some", "somebody", "someone", "something", "somewhat",
         "such", "suchlike", "that", "thee", "their", "theirs", "theirself",
         "theirselves", "them", "themself", "themselves", "there", "these", "they",
         "thine", "this", "those", "thou", "thy", "thyself", "us", "we", "what",
         "whatever", "whatnot", "whatsoever", "whence", "where", "whereby",
         "wherefrom", "wherein", "whereinto", "whereof", "whereon", "wherever",
         "wheresoever", "whereto", "whereunto", "wherewith", "wherewithal",
         "whether", "which", "whichever", "whichsoever", "who", "whoever", "whom",
         "whomever", "whomso", "whomsoever", "whose", "whosever", "whosesoever",
         "whoso", "whosoever", "ye", "yon", "yonder", "you", "your", "yours",
         "yourself", "yourselves"
      };


      public static IEnumerable SplitString(string s,
         bool excludeArticles,
         bool excludeConjunctions,
         bool excludePrepositions,
         bool excludePronouns,
         bool distinctOnly)
      {
         if (string.IsNullOrWhiteSpace(s))
            return null;

         double i;

         string[] words = s.Split(
            new char[] { ' ', '\r', '\n', '\f', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

         // remove leading and trailing punctuation from each word
         for (int idx = 0; idx < words.Length; idx++)
         {
            while (words[idx].Length > 0  &&  char.IsPunctuation(words[idx].First()))
               words[idx] = words[idx].Substring(1);

            while (words[idx].Length > 0  &&  char.IsPunctuation(words[idx].Last()))
               words[idx] = words[idx].Substring(0, words[idx].Length - 1);

            words[idx] = words[idx].Replace("’", "'").Replace("‘", "'");

            if (words[idx].EndsWith("'s"))
               words[idx] = words[idx].Substring(0, words[idx].Length - 2);
         }

         if (excludeArticles)
            words = words.Where(w => !Articles.Contains(w.ToLower())).ToArray();

         if (excludeConjunctions)
            words = words.Where(w => !Conjunctions.Contains(w.ToLower())).ToArray();

         if (excludePrepositions)
            words = words.Where(w => !Prepositions.Contains(w.ToLower())).ToArray();

         if (excludePronouns)
            words = words.Where(w => !Pronouns.Contains(w.ToLower())).ToArray();

         // remove one-letter words and numbers
         words = words.Where(w => w.Length > 1  &&  !double.TryParse(w, out i)).ToArray();

         if (distinctOnly)
            words = words.Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();

         if (words.Length == 0)
            return null;
         else
            return words;
      }


      [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      [SqlFunction(IsDeterministic = true, IsPrecise = true,
         DataAccess = DataAccessKind.None,
         SystemDataAccess = SystemDataAccessKind.None,
         Name = "UdfSplitString",
         FillRowMethodName = "NextFillRow")]
      public static IEnumerable UdfSplitString(SqlString s,
         SqlBoolean excludeArticles,
         SqlBoolean excludeConjunctions,
         SqlBoolean excludePrepositions,
         SqlBoolean excludePronouns,
         SqlBoolean distinctOnly)
      {
         if (s == null  ||  string.IsNullOrWhiteSpace(s.Value))
            return null;

         return SplitString(s.Value, excludeArticles.Value,
            excludeConjunctions.Value, excludePrepositions.Value,
            excludePronouns.Value, distinctOnly.Value);
      }


      [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      private static void NextFillRow(object nextRow, out SqlString word)
      {
         word = new SqlString((string)nextRow);
      }
   }
}