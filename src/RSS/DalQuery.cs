using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;


namespace RSS
{
   public class DalQuery : Dal
   {
      public DalQuery(DbConnection db) : base(db)
      {
      }


      #region apiMethods
      /// <summary>
      /// Retrieve a list of items whose titles contain one or more of the
      /// specified space-separated keywords.
      /// </summary>
      public IReadOnlyList<ItemRow> GetItemsByKeywords(string keywords)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            cmd = _dbConn.CreateCommand();

            // use case-insensitive collation for LIKE
            cmd.CommandText = "SELECT " + ColumnList +
               " FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id AND EXISTS " +
               "(SELECT value FROM STRING_SPLIT(@keywords, ' ') WHERE TRIM(value) != '' AND i.title COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + value + '%')";

            cmd.Parameters.Add(CreateParameter(cmd, "@keywords", DbType.String, keywords.ToLower()));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            var items = ReadToItemList(dr);
            dr.Close();

            return (items != null  &&  items.Count > 0) ? items : null;
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Return a list of items whose publication date falls between
      /// <paramref name="lowDate"/> and <paramref name="highDate"/>, inclusively.
      /// </summary>
      public IReadOnlyList<ItemRow> GetItemsByRange(DateTime lowDate, DateTime highDate)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (lowDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("lowDate must be UTC", nameof(lowDate));

         if (highDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("highDate must be UTC", nameof(highDate));

         if (highDate <= lowDate)
            throw new ArgumentException("highDate must be greater than lowDate");

         try
         {
            cmd = _dbConn.CreateCommand();

            cmd.CommandText = "SELECT " + ColumnList +
               " FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id AND i.pub_date >= @lowDate AND i.pub_date <= @highDate ORDER BY i.ins_date";

            cmd.Parameters.Add(CreateParameter(cmd, "@lowDate", DbType.DateTime2, lowDate));
            cmd.Parameters.Add(CreateParameter(cmd, "@highDate", DbType.DateTime2, highDate));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            var items = ReadToItemList(dr);
            dr.Close();

            return ((items != null  &&  items.Count > 0) ? items : null);
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }
      #endregion


      #region webDemoMethods
      /// <summary>
      /// Retrieve a list of items whose title or desciption contains the
      /// given state's full name.
      /// </summary>
      public IReadOnlyList<ItemRow> GetItemsByState(string abbr, int count)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (string.IsNullOrEmpty(abbr))
            throw new ArgumentException("abbr is a required parameter", nameof(abbr));

         if (count <= 0)
            throw new ArgumentException("count must be > 0", nameof(count));

         try
         {
            cmd = _dbConn.CreateCommand();

            cmd.CommandText = "SELECT TOP (@count) " + ColumnList +
               " FROM rss_item i, rss_feed f, state s WHERE s.abbr = @abbr AND " +
               "i.feed_id = f.feed_id AND f.regional = 0 AND " +
               "(i.title COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + s.name + '%' OR i.description COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + s.name + '%') " +
               "ORDER BY i.ins_date DESC";

            cmd.Parameters.Add(CreateParameter(cmd, "@count", DbType.Int32, count));
            cmd.Parameters.Add(CreateParameter(cmd, "@abbr", DbType.String, abbr));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            var items = ReadToItemList(dr);
            dr.Close();

            return ((items != null  &&  items.Count > 0) ? items : null);
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Returns a set of totals which represent the number of items published
      /// by non-regional feeds within the past <paramref name="hours"/>N hours
      /// which contain a state name.
      /// </summary>
      public IReadOnlyList<StateActivity> GetStateActivityByRange(int hours)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (hours <= 0)
            throw new ArgumentException("hours must be > 0", nameof(hours));

         try
         {
            cmd = _dbConn.CreateCommand();

            cmd.CommandText = "WITH tbl AS (SELECT i.title, i.description FROM rss_item i, rss_feed f WHERE f.feed_id = i.feed_id and f.regional = 0 AND i.ins_date > DATEADD(HOUR, @hour, GETUTCDATE())) " +
               "SELECT s.abbr, COUNT(*) FROM tbl, state s WHERE tbl.title COLLATE sql_latin1_general_cp1_ci_as LIKE '% + s.name + %' OR tbl.description COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + s.name + '%' " +
               "GROUP BY s.abbr ORDER BY COUNT(*) DESC, s.abbr ASC";

            cmd.Parameters.Add(CreateParameter(cmd, "@hour", DbType.Int32, -hours));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);
            List<StateActivity> vals = new List<StateActivity>();

            while (dr.Read())
               vals.Add(new StateActivity() { StateAbbr = dr.GetString(0), Count = dr.GetInt32(1) });

            return (vals.Count > 0 ? vals : null);
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Similar to <see cref="GetStateActivityByRange"/>, except that instead
      /// of returning a count set based on a single <paramref name="hours"/>
      /// range, this returns a list of <paramref name="historySetCount"/> activity
      /// sets, each overlapping the next by one hour, with the last set being
      /// equivalent to that which would have been returned by
      /// <see cref="GetStateActivityByRange"/>.
      /// </summary>
      public IReadOnlyDictionary<DateTime, IReadOnlyList<StateActivity>> 
         GetStateActivityHistory(int hours, int historySetCount)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (hours <= 0)
            throw new ArgumentException("hours must be > 0", nameof(hours));

         if (historySetCount <= 0)
            throw new ArgumentException("historySetCount must be > 0", nameof(historySetCount));

         try
         {
            cmd = _dbConn.CreateCommand();

            // 'items' is the subset of rss_item rows to be examined.
            // 'hours' is a set of timestamps to be used for time blocking each
            // activity set (note that this CTE is recursive).
            cmd.CommandText = @"
WITH items AS (
   SELECT 
      i.ins_date AS ins_date, s.abbr AS abbr
   FROM 
      rss_item i, rss_feed f, state s 
   WHERE 
      f.feed_id = i.feed_id AND 
      f.regional = 0 AND 
      i.ins_date > DATEADD(HOUR, -@setCount, GETUTCDATE()) AND 
      (i.title COLLATE sql_latin1_general_cp1_ci_as LIKE '% + s.name + %' OR 
      i.description COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + s.name + '%')
),
hours AS (
   SELECT DATEADD(HOUR, -@setCount, GETUTCDATE()) AS start_hour
   UNION ALL
   SELECT DATEADD(HH, 1, start_hour) FROM hours WHERE start_hour < GETUTCDATE()
)
SELECT
   start_hour, items.abbr, COUNT(*) 
FROM 
   hours, items 
WHERE 
   items.ins_date > DATEADD(HOUR, -@hours, start_hour) AND 
   items.ins_date <= start_hour 
GROUP BY 
   start_hour, items.abbr 
ORDER BY
   start_hour, COUNT(*) DESC
OPTION (MAXRECURSION 0)";

            cmd.Parameters.Add(CreateParameter(cmd, "@hours", DbType.Int32, hours));
            cmd.Parameters.Add(CreateParameter(cmd, "@setCount", DbType.Int32, historySetCount));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            Dictionary<DateTime, IReadOnlyList<StateActivity>> sets =
               new Dictionary<DateTime, IReadOnlyList<StateActivity>>();

            List<StateActivity> set = new List<StateActivity>();
            DateTime setDate = DateTime.MinValue;
            int rowCount = 0;

            while (dr.Read())
            {
               rowCount++;
               DateTime d = DateTime.SpecifyKind(dr.GetDateTime(0), DateTimeKind.Utc);

               if (setDate == DateTime.MinValue)
                  setDate = d;

               // hour changed, so start a new activity set
               if (setDate.ToString("yyyyMMddHH") != d.ToString("yyyyMMddHH"))
               {
                  sets.Add(setDate, set);
                  setDate = d;
                  set = new List<StateActivity>();
               }

               set.Add(new StateActivity() { StateAbbr = dr.GetString(1), Count = dr.GetInt32(2) });
            }

            // add last set
            sets.Add(setDate, set);

            return sets;
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Returns a list of the 100 most frequently occurring words in the title
      /// and description of items retrieved within the last 
      /// <paramref name="hours">N hours</paramref>.
      /// </summary>
      public IReadOnlyList<WordCount> GetTrendingWords(int hours, bool filterCommonWords)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (hours <= 0)
            throw new ArgumentException("hours must be > 0", nameof(hours));

         try
         {
            cmd = _dbConn.CreateCommand();

            // determine whether UdfSplitString is installed on the target
            // SQL Server
            cmd.CommandText = @"SELECT COUNT(*) FROM information_schema.routines WHERE 
               routine_name = 'UdfSplitString'";

            int count = (int)cmd.ExecuteScalar();

            // if not, then use equivalent in-code logic
            if (count < 1)
               return GetTrendingWordsLinq(hours, filterCommonWords);

            cmd.CommandText = @"SELECT TOP 100 CAST(word AS NVARCHAR(60)) [Keyword], COUNT(*) AS [Count] FROM 
rss_item AS i INNER JOIN rss_feed AS f ON 
(f.feed_id = i.feed_id AND f.regional = 0) CROSS APPLY
dbo.UdfSplitString(
   LOWER(
      i.title + ' ' +
      CAST(CAST(REPLACE(COALESCE(i.description, ''), '&', '&amp;') AS XML).query('/text()') AS NVARCHAR(MAX))
   ),
   1, 1, 1, 1, 1)
WHERE i.ins_date > DATEADD(HOUR, -@hour, GETUTCDATE()) AND word NOT IN (SELECT iw.word FROM ignore_word AS iw) " +
               (filterCommonWords ? " AND word NOT IN (SELECT cw.word FROM common_word AS cw) " : "") +
               " GROUP BY word ORDER BY COUNT(*) DESC";

            cmd.Parameters.Add(CreateParameter(cmd, "@hour", DbType.Int32, hours));

            List<WordCount> wordCounts = new List<WordCount>();
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            while (dr.Read())
               wordCounts.Add(new WordCount() { Word = dr.GetString(0), Count = dr.GetInt32(1) });

            return (wordCounts.Count > 0 ? wordCounts : null);
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Alternate method of retrieving trending words, using multiple SQL
      /// queries and LINQ.
      /// </summary>
      private IReadOnlyList<WordCount> GetTrendingWordsLinq(int hours, bool filterCommonWords)
      {
         var ignorableWords = GetIgnorableWords();
         IReadOnlyList<string> commonWords = new List<string>();
         
         if (filterCommonWords)
            commonWords = GetCommonWords();

         var feeds = GetFeeds();

         var items = GetItemsByRange(
            DateTime.UtcNow.AddHours(-hours), DateTime.UtcNow);

         if (items == null)
            return null;

         // filter out items from regional feeds, and split the title/desc
         // text into a set of distinct word arrays
         var splits = items.Where(item => (feeds.Where(f => f.FeedId == item.FeedId).First().IsRegional == false))
            .Select(i => SQLServerUDF.SQLServerUDF.SplitString(
               i.Title.ToLower() + " " + i.DescriptionInnerText().ToLower(),
               true, true, true, true, true));

         // flatten the word arrays into a single array, filter out
         // ignorable words, group and select all remaining words by count,
         // sort and take the top 100
         var words = splits.SelectMany(split => split.Cast<string>().ToArray())
            .Where(s => !ignorableWords.Contains(s))
            .Where(s => !commonWords.Contains(s))
            .GroupBy(g => g)
            .Select(o => new { word = o.Key, count = o.Count() })
            .OrderByDescending(d => d.count)
            .Take(100);

         List<WordCount> wordCounts = new List<WordCount>();

         foreach (var word in words)
            wordCounts.Add(new WordCount() { Word = word.word, Count = word.count });

         return (wordCounts.Count > 0 ? wordCounts : null);
      }


      /// <summary>
      /// Retrieve hourly usage counts for a given word, spaning the past 
      /// <paramref name="hours">hours</paramref>. Note that the number of counts
      /// will total N+1 hours, to account for the partial hour which has elapsed
      /// at the time the method is called.
      /// </summary>
      /// <returns>A list of date/count combos, in which the date is formatted as
      /// yyyyMMddHH (military hours). and the counts represent hourly usage.</returns>
      public IReadOnlyList<HourlyWordUsage> GetWordHistory(string word, int hours)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (hours <= 0)
            throw new ArgumentException("hours must be > 0", nameof(hours));

         try
         {
            cmd = _dbConn.CreateCommand();

            cmd.CommandText = @"SELECT ins_date, i.title, description FROM rss_item i 
               INNER JOIN rss_feed AS f ON (f.feed_id = i.feed_id AND f.regional = 0) WHERE
               i.ins_date > DATEADD(HOUR, -@h, GETUTCDATE()) AND LOWER(i.title + ' ' + i.description) LIKE '%' + @word + '%'";

            cmd.Parameters.Add(CreateParameter(cmd, "@h", DbType.Int32, hours));
            cmd.Parameters.Add(CreateParameter(cmd, "@word", DbType.String, word.ToLower()));

            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            DateTime start = DateTime.UtcNow.AddHours(-hours);
            List<HourlyWordUsage> counts = new List<HourlyWordUsage>();

            // add an extra data point to the result set to account for the minutes
            // since the clock last chimed a full hour
            for (int h = 0; h < hours + 1; h++)
            {
               counts.Add(new HourlyWordUsage() { DateAndHour = start.ToString("yyyyMMddHH"), Count = 0 });
               start = start.AddHours(1);
            }

            while (dr.Read())
            {
               DateTime insDate = DateTime.SpecifyKind(dr.GetDateTime(0), DateTimeKind.Utc);
               string dayHour = insDate.ToString("yyyyMMddHH");
               string title = dr.GetString(1).ToLower();
               string desc = dr.GetString(2).ToLower(); ;

               var words = SQLServerUDF.SQLServerUDF.SplitString(title + " " + desc,
                  true, true, true, true, true);

               string[] wordArray = words.Cast<string>().ToArray();

               if (!wordArray.Contains(word))
                  continue;

               IEnumerable<HourlyWordUsage> count = counts.Where(c => c.DateAndHour == dayHour);

               if (count != null  &&  count.Count() > 0)
               {
                  foreach (HourlyWordUsage h in count)
                     h.Count++;
               }

               //if (counts.Contains(c => c.DateAndHour == dayHour))
               //   counts[dayHour]++;
            }

            return (counts.Count > 0 ? counts : null);
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Retrieve all words from the ignore_word table.
      /// </summary>
      public IReadOnlyList<string> GetIgnorableWords()
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            List<string> words = new List<string>();

            cmd = _dbConn.CreateCommand();
            cmd.CommandText = @"SELECT word FROM ignore_word";
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            while (dr.Read())
               words.Add(dr.GetString(0));

            return words;
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Retrieve all words from the common_word table.
      /// </summary>
      public IReadOnlyList<string> GetCommonWords()
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            List<string> words = new List<string>();

            cmd = _dbConn.CreateCommand();
            cmd.CommandText = @"SELECT word FROM common_word";
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            while (dr.Read())
               words.Add(dr.GetString(0));

            return words;
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Record the 100 most commonly-used words in all items stored in the
      /// rss_item table.
      /// </summary>
      public void UpdateCommonWords()
      {
         DbCommand cmd = null;
         DbDataReader dr = null;
         DbTransaction t = null;

         try
         {
            t = _dbConn.BeginTransaction();
            cmd = _dbConn.CreateCommand();
            cmd.CommandText = @"DELETE FROM common_word";
            cmd.Transaction = t;
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"INSERT INTO common_word (word) SELECT TOP 100 CAST(word AS NVARCHAR(60)) [Keyword] FROM 
               rss_item AS i INNER JOIN rss_feed AS f ON 
               (f.feed_id = i.feed_id AND f.regional = 0) CROSS APPLY
               dbo.UdfSplitString(
                  LOWER(
                     i.title + ' ' +
                     CAST(CAST(REPLACE(COALESCE(i.description, ''), '&', '&amp;') AS XML).query('/text()') AS NVARCHAR(MAX))
                  ),
                  1, 1, 1, 1, 1) 
               GROUP BY word ORDER BY COUNT(*) DESC";

            int i = cmd.ExecuteNonQuery();

            if (i > 0)
            {
               t.Commit();
               t.Dispose();
               t = null;
            }
         }
         finally
         {
            t?.Rollback();
            t?.Dispose();
            t = null;

            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Return a list of item titles which contain profanity, with the
      /// profane words bleeped.
      /// </summary>
      public IReadOnlyList<string> GetBleepedTitles()
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            cmd = _dbConn.CreateCommand();

            // determine whether UdfBleep is installed on the target
            // SQL Server
            cmd.CommandText = @"SELECT COUNT(*) FROM information_schema.routines WHERE 
               routine_name = 'UdfBleep'";

            int count = (int)cmd.ExecuteScalar();

            // if not, then use equivalent in-code logic
            if (count < 1)
               return GetBleepedTitlesLinq();

            cmd.CommandText = "SELECT dbo.UdfBleep(title) FROM rss_item WHERE dbo.UdfNeedsBleeping(title) = 1";

            List<string> titles = new List<string>();
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            while (dr.Read())
               titles.Add(dr.GetString(0));

            return (titles.Count > 0 ? titles : null);
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }



      /// <summary>
      /// Alternate method of retrieving profane titles, using multiple SQL
      /// queries and LINQ.
      /// </summary>
      private IReadOnlyList<string> GetBleepedTitlesLinq()
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            cmd = _dbConn.CreateCommand();
            cmd.CommandText = "SELECT word FROM profanity";
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);
            List<string> profanity = new List<string>();

            while (dr.Read())
               profanity.Add(dr.GetString(0));

            dr.Close();

            if (profanity.Count <= 0)
               return null;

            cmd.CommandText = "SELECT DISTINCT i.title FROM rss_item i, profanity p WHERE LOWER(i.title) LIKE '%' + p.word + '%'";
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);
            List<string> titles = new List<string>();

            while (dr.Read())
               titles.Add(dr.GetString(0));

            dr.Close();

            if (titles.Count <= 0)
               return null;

            var bleeped = titles
               .Where(o => SQLServerUDF.SQLServerUDF.NeedsBleeping(profanity, o))
               .Select(b => SQLServerUDF.SQLServerUDF.Bleep(profanity, b));

            return bleeped.ToList();
         }
         finally
         {
            Cleanup(cmd, dr);
         }
      }


      /// <summary>
      /// Execute an arbitrary SQL stored procedure.
      /// 
      /// Um, don't put something like this in a real app, m'kay?
      /// </summary>
      public DataTable SelectExec(string procName)
      {
         DbCommand cmd = null;

         try
         {
            cmd = _dbConn.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;

            return SelectToDataTable(cmd);
         }
         finally
         {
            Cleanup(cmd, null);
         }
      }


      public DataTable SelectByKeyword(string keyword)
      {
         DbCommand cmd = null;

         try
         {
            cmd = _dbConn.CreateCommand();

            cmd.CommandText = @"SELECT title [Title] FROM rss_item WHERE 
               title COLLATE sql_latin1_general_cp1_ci_as LIKE @tk OR 
               xml.exist('/item/description/text()[contains(lower-case(.), sql:variable(""@dk""))]') = 1";

            cmd.Parameters.Add(CreateParameter(cmd, "@tk", DbType.String, "%" + keyword.ToLower() + "%"));
            cmd.Parameters.Add(CreateParameter(cmd, "@dk", DbType.String, keyword.ToLower()));

            return SelectToDataTable(cmd);
         }
         finally
         {
            Cleanup(cmd, null);
         }
      }


      private static DataTable SelectToDataTable(DbCommand cmd)
      {
         DbDataReader dr = null;

         try
         {
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            if (dr.HasRows)
            {
               DataTable dt = new DataTable();
               dt.Load(dr);

               return dt;
            }
            else
               return null;
         }
         finally
         {
            Cleanup(null, dr);
         }
      }
      #endregion
   }
}
