using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;

using Microsoft.SqlServer.Server;


namespace SQLServerUDF
{
   /// <summary>
   /// If supported by the target instance of SQL Server, this class may be
   /// compiled as a separate library and installed as a CLR assembly of
   /// user-defined functions using the following instructions.

   /// To compile:
   /// 
   /// c:\windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:library SQLServerUDF.cs
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
   /// DROP FUNCTION dbo.UdfNeedsBleeping;
   /// go
   /// DROP FUNCTION dbo.UdfBleep;
   /// go
   /// DROP ASSEMBLY StringUtils;
   /// go
   /// CREATE ASSEMBLY StringUtils 
   ///    FROM 'c:\data\source\Portfolio\SQLServerUDF\bin\Debug\SQLServerUDF.dll' WITH PERMISSION_SET = SAFE;
   /// go
   /// CREATE FUNCTION UdfSplitString(@s NVARCHAR(MAX), @excludeArticles BIT, @excludeConjunctions BIT, @excludePrepositions BIT, @excludePronouns BIT, @distinctOnly BIT) 
   ///    RETURNS TABLE(word NVARCHAR(MAX)) AS EXTERNAL NAME StringUtils.[SQLServerUDF.SQLServerUDF].UdfSplitString;
   /// go
   /// CREATE FUNCTION UdfNeedsBleeping(@s NVARCHAR(MAX)) 
   ///    RETURNS BIT AS EXTERNAL NAME StringUtils.[SQLServerUDF.SQLServerUDF].UdfNeedsBleeping;
   /// go
   /// CREATE FUNCTION UdfBleep(@s NVARCHAR(MAX)) 
   ///    RETURNS NVARCHAR(MAX) AS EXTERNAL NAME StringUtils.[SQLServerUDF.SQLServerUDF].UdfBleep;
   /// go
   /// 
   /// Usage (find top 100 most frequently-used words in recent items):
   /// 
   /// SELECT TOP 100 CAST(word AS NVARCHAR(60)), COUNT(*) FROM rss_item AS i 
   ///    CROSS APPLY dbo.UdfSplitString(LOWER(i.title) + ' ' + LOWER(COALESCE(i.description, '')), 1, 1, 1, 1, 1) WHERE
   ///    i.ins_date > '2021-5-20' GROUP BY word ORDER BY COUNT(*) DESC;
   /// </summary>
   public static class SQLServerUDF
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


      [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      [SqlFunction(IsDeterministic = true, IsPrecise = true,
         DataAccess = DataAccessKind.Read,
         SystemDataAccess = SystemDataAccessKind.None,
         Name = "UdfNeedsBleeping")]
      public static SqlBoolean UdfNeedsBleeping(SqlString s)
      {
         if (s == null  ||  string.IsNullOrWhiteSpace(s.Value))
            return SqlBoolean.False;

         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection("context connection=true");
            dbConn.Open();

            string[] profanity = GetProfanity(dbConn).ToArray();

            return NeedsBleeping(profanity, s.Value);
         }
         finally
         {
            if (dbConn != null)
            {
               dbConn.Close();
               dbConn.Dispose();
            }

            dbConn = null;
         }
      }


      [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
      [SqlFunction(IsDeterministic = true, IsPrecise = true,
         DataAccess = DataAccessKind.Read,
         SystemDataAccess = SystemDataAccessKind.None,
         Name = "UdfBleep")]
      public static SqlString UdfBleep(SqlString s)
      {
         if (s == null  ||  string.IsNullOrWhiteSpace(s.Value))
            return s == null ? SqlString.Null : s;

         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection("context connection=true");
            dbConn.Open();

            string[] profanity = GetProfanity(dbConn).ToArray();

            return new SqlString(Bleep(profanity, s.Value));
         }
         finally
         {
            if (dbConn != null)
            {
               dbConn.Close();
               dbConn.Dispose();
            }

            dbConn = null;
         }
      }


      public static IEnumerable SplitString(string s,
         bool excludeArticles,
         bool excludeConjunctions,
         bool excludePrepositions,
         bool excludePronouns,
         bool distinctOnly)
      {
         if (string.IsNullOrWhiteSpace(s))
            return null;

         string[] words = Split(s);

         for (int idx = 0; idx < words.Length; idx++)
            words[idx] = TrimPunctuation(words[idx]).ToLower();

         if (excludeArticles)
            words = words.Where(w => !Articles.Contains(w)).ToArray();

         if (excludeConjunctions)
            words = words.Where(w => !Conjunctions.Contains(w)).ToArray();

         if (excludePrepositions)
            words = words.Where(w => !Prepositions.Contains(w)).ToArray();

         if (excludePronouns)
            words = words.Where(w => !Pronouns.Contains(w)).ToArray();

         // remove one-letter words and numbers
         words = words.Where(w => w.Length > 1  &&  !double.TryParse(w, out double i)).ToArray();

         if (distinctOnly)
            words = words.Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();

         if (words.Length == 0)
            return null;
         else
            return words;
      }


      private static IReadOnlyList<string> GetProfanity(SqlConnection dbConn)
      {
         List<string> profanity = new List<string>();
         SqlCommand cmd = null;
         SqlDataReader dr = null;

         try
         {
            cmd = new SqlCommand("SELECT word FROM profanity", dbConn);
            dr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleResult);

            while (dr.Read())
               profanity.Add(dr.GetString(0));

            return profanity;
         }
         finally
         {
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
            }

            dr = null;

            if (cmd != null)
               cmd.Dispose();

            cmd = null;
         }
      }


      public static bool NeedsBleeping(IReadOnlyList<string> profanity, string s)
      {
         if (string.IsNullOrWhiteSpace(s))
            return false;

         if (profanity.Count <= 0)
            return false;

         string[] words = Split(s);

         // this looks really elegant, but it's much slower than brute iteration
         //return words.Where(w => profanity.Contains(TrimPunctuation(w).ToLower())).Count() > 0;

         for (int idx = 0; idx < words.Length; idx++)
            words[idx] = TrimPunctuation(words[idx]).ToLower();

         foreach (string w in words)
         {
            foreach (string p in profanity)
            {
               if (w == p)
                  return true;
            }
         }

         return false;
      }


      /// <summary>
      /// Returns d--m instead of damn, h--l instead of hell, etc.
      /// </summary>
      /// <returns></returns>
      public static string Bleep(IReadOnlyList<string> profanity, string s)
      {
         if (string.IsNullOrWhiteSpace(s))
            return s;

         if (profanity.Count <= 0)
            return s;

         string[] words = Split(s);

         for (int i = 0; i < words.Length; i++)
         {
            string trimmedWord = TrimPunctuation(words[i]).ToLower();

            foreach (var p in profanity)
            {
               if (trimmedWord == p)
               {
                  char[] arr = words[i].ToCharArray();
                  int pStart = words[i].IndexOf(p, StringComparison.InvariantCultureIgnoreCase);

                  // blank out interior of profane word
                  for (int cIdx = 0; cIdx < p.Length - 2; cIdx++)
                     arr[cIdx + pStart + 1] = '-';

                  s = s.Replace(words[i], new string(arr));
               }
            }
         }

         return s;
      }


      private static string[] Split(string s)
      {
         if (string.IsNullOrWhiteSpace(s))
            return null;

         return s.Split(new char[] { ' ', '\r', '\n', '\f', '\t' },
            StringSplitOptions.RemoveEmptyEntries);
      }


      private static string TrimPunctuation(string s)
      {
         // remove leading and trailing punctuation
         while (s.Length > 0  &&  char.IsPunctuation(s.First()))
            s = s.Substring(1);

         while (s.Length > 0  &&  char.IsPunctuation(s.Last()))
            s = s.Substring(0, s.Length - 1);

         s = s.Replace("’", "'").Replace("‘", "'");

         if (s.EndsWith("'s"))
            s = s.Substring(0, s.Length - 2);

         return s;
      }
   }
}