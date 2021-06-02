// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;


namespace RSS
{
   public class Dal : IDal
   {
      private readonly DbConnection _dbConn;

      private readonly string ColumnList =
         "i.feed_id, f.title, i.ins_date, i.pub_date, i.title, i.description, i.url, i.xml";

      private static List<string> _stateNames;


      public Dal(DbConnection inConn)
      {
         _dbConn = inConn;

         // load once on app/site initialization
         if (_stateNames == null)
         {
            DbCommand cmd = null;
            DbDataReader dr = null;

            try
            {
               if (_dbConn.State != ConnectionState.Open)
                  _dbConn.Open();
   
               cmd = _dbConn.CreateCommand();
               cmd.CommandText = "SELECT name FROM state";
               dr = cmd.ExecuteReader(CommandBehavior.SingleResult);
               _stateNames = new List<string>();

               while (dr.Read())
                  _stateNames.Add(dr.GetString(0));
            }
            finally
            {
               if (dr != null)
               {
                  dr.Close();
                  dr.Dispose();
                  dr = null;
               }

               if (cmd != null)
               {
                  cmd.Dispose();
                  cmd = null;
               }
            }
         }
      }


      /// <summary>
      /// Add a new row to the rss_feed table.
      /// </summary>
      /// <param name="title">Feed name</param>
      /// <param name="url">Feel URL</param>
      /// <param name="isRegional">Is the feed primarily concerned with regional news?</param>
      /// <returns>Success state of the transaction</returns>
      public bool InsertFeed(string title, string url, bool isRegional)
      {
         DbCommand cmd = null;

         try
         {
            cmd = _dbConn.CreateCommand();
            cmd.CommandText = "INSERT INTO rss_feed (title, url, regional, active) VALUES (@title, @url, @regional, 1)";
            cmd.Parameters.Add(CreateParameter(cmd, "@title", DbType.String, title));
            cmd.Parameters.Add(CreateParameter(cmd, "@url", DbType.String, url));
            cmd.Parameters.Add(CreateParameter(cmd, "@regional", DbType.Boolean, isRegional));

            return (cmd.ExecuteNonQuery() == 1);
         }
         finally
         {
            cmd?.Dispose();
         }
      }


      /// <summary>
      /// Delete the specified feed, and all items associated with that
      /// feed. This is a transactional action.
      /// </summary>
      public bool DeleteFeed(int feedId)
      {
         DbCommand cmd = null;
         DbTransaction t = null;

         try
         {
            t = _dbConn.BeginTransaction();

            cmd = _dbConn.CreateCommand();
            cmd.CommandText = "DELETE FROM rss_item WHERE feed_id = @id";
            cmd.Parameters.Add(CreateParameter(cmd, "@id", DbType.Int32, feedId));
            cmd.Transaction = t;

            // there might not be any associated items, so don't rollback if
            // the return code is < 1
            cmd.ExecuteNonQuery();

            cmd = _dbConn.CreateCommand();
            cmd.CommandText = "DELETE FROM rss_feed WHERE feed_id = @id";
            cmd.Parameters.Add(CreateParameter(cmd, "@id", DbType.Int32, feedId));
            cmd.Transaction = t;

            int rc = cmd.ExecuteNonQuery();

            if (rc == 1)
            {
               t.Commit();
               t.Dispose();
               t = null;

               return true;
            }
            else
            {
               t.Rollback();
               t.Dispose();
               t = null;
            }
         }
         finally
         {
            if (t != null)
            {
               t.Rollback();
               t.Dispose();
            }

            cmd?.Dispose();
         }

         return false;
      }


      public bool EditFeed(int feedId, string title, string url, bool isRegional,
         bool isActive)
      {
         DbCommand cmd = null;

         try
         {
            cmd = _dbConn.CreateCommand();
            cmd.CommandText = "UPDATE rss_feed SET title = @title, url = @url, regional = @regional, active = @active WHERE feed_id = @id";
            cmd.Parameters.Add(CreateParameter(cmd, "@title", DbType.String, title));
            cmd.Parameters.Add(CreateParameter(cmd, "@url", DbType.String, url));
            cmd.Parameters.Add(CreateParameter(cmd, "@regional", DbType.Boolean, isRegional));
            cmd.Parameters.Add(CreateParameter(cmd, "@active", DbType.Boolean, isActive));
            cmd.Parameters.Add(CreateParameter(cmd, "@id", DbType.Int32, feedId));

            return (cmd.ExecuteNonQuery() == 1);
         }
         finally
         {
            cmd?.Dispose();
         }
      }


      /// <summary>
      /// Get a list of all feeds.
      /// </summary>
      public IReadOnlyList<FeedRow> GetFeeds()
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            List<FeedRow> items = new List<FeedRow>();

            cmd = _dbConn.CreateCommand();
            cmd.CommandText = "SELECT feed_id, title, url, regional, active FROM rss_feed";
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            if (!dr.HasRows)
               return null;

            while (dr.Read())
            {
               FeedRow item = new FeedRow(dr.GetInt32(0), dr.GetString(1),
                  dr.GetString(2), dr.GetBoolean(3), dr.GetBoolean(4));

               items.Add(item);
            }

            dr.Close();

            return (items.Count > 0 ? items : null);
         }
         finally
         {
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
            }

            cmd?.Dispose();
         }
      }


      /// <summary>
      /// Add a row to the rss_item table.
      /// </summary>
      /// <param name="feedId">Feed ID, foreign-keyed to rss_feed table.</param>
      /// <param name="item">Item information.</param>
      /// <returns>Success state of the transaction</returns>
      public bool InsertItem(int feedId, Item item)
      {
         if (item.PubDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("pubDate must be UTC", nameof(item.PubDate));

         DbCommand cmd = null;
         DbTransaction t = null;

         try
         {
            DateTime maxDate = GetMaxInsertDate();
            DateTime insDate = DateTime.UtcNow;

            while (insDate <= maxDate)
               insDate = DateTime.UtcNow;

            t = _dbConn.BeginTransaction();
            cmd = _dbConn.CreateCommand();
            cmd.Transaction = t;

            cmd.CommandText =
               "INSERT INTO rss_item (feed_id, ins_date, pub_date, title, description, url, xml) VALUES " +
               "(@feedid, @insDate, @pubdate, @title, @desc, @url, @xml)";

            cmd.Parameters.Add(CreateParameter(cmd, "@feedid", DbType.Int32, feedId));
            cmd.Parameters.Add(CreateParameter(cmd, "@insdate", DbType.DateTime2, insDate));
            cmd.Parameters.Add(CreateParameter(cmd, "@pubdate", DbType.DateTime2, item.PubDate));
            cmd.Parameters.Add(CreateParameter(cmd, "@title", DbType.String, item.Title.Trim()));
            cmd.Parameters.Add(CreateParameter(cmd, "@desc", DbType.String, ("" + item.Description).Trim()));
            cmd.Parameters.Add(CreateParameter(cmd, "@url", DbType.String, item.Url));
            cmd.Parameters.Add(CreateParameter(cmd, "@xml", DbType.Xml, item.ItemXml));

            int rc = cmd.ExecuteNonQuery();

            if (rc != 1)
               return false;

            // increment the state table's grand totals, if  one of more state names
            // are referenced in the current item
            foreach (string name in _stateNames)
            {
               if (item.Title.ToUpper().Contains(name)  ||
                  ("" + item.Description).ToUpper().Contains(name))
               {
                  cmd.CommandText = "UPDATE state SET ref_count = ref_count + 1 WHERE " +
                     "name = @name AND " +
                     "EXISTS (SELECT regional FROM rss_feed WHERE feed_id = @id AND regional = 0)";

                  cmd.Parameters.Clear();
                  cmd.Parameters.Add(CreateParameter(cmd, "@name", DbType.String, name));
                  cmd.Parameters.Add(CreateParameter(cmd, "@id", DbType.Int32, feedId));
                  rc = cmd.ExecuteNonQuery();

                  if (rc != 1)
                     return false;
               }
            }

            t.Commit();
            t.Dispose();
            t = null;

            return true;
         }
         finally
         {
            if (t != null)
            {
               t.Rollback();
               t.Dispose();
               t = null;
            }
            
            cmd?.Dispose();
         }
      }


      /// <summary>
      /// Get a list of the N newest news items.
      /// </summary>
      /// <param name="itemCount">Number of items to return</param>
      /// <returns>List of items if any exist, otherwise NULL.</returns>
      public IReadOnlyList<ItemRow> GetTopItems(int itemCount)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (itemCount <= 0)
            throw new ArgumentException("itemCount must be > 0", nameof(itemCount));

         try
         {
            cmd = _dbConn.CreateCommand();

            cmd.CommandText = "SELECT TOP (@count) " + ColumnList +
               " FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id ORDER BY i.ins_date DESC";

            cmd.Parameters.Add(CreateParameter(cmd, "@count", DbType.Int32, itemCount));

            cmd.CommandTimeout = 100000;
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            var items = ReadToItemList(dr);
            dr.Close();

            if (items == null  ||  items.Count == 0)
               return null;
            else
            {
               // sort by pubDate
               var sorted = from i in items orderby i.PubDate descending select i;

               return sorted.ToArray();
            }
         }
         finally
         {
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
            }
            
            cmd?.Dispose();
         }
      }


      /// <summary>
      /// Get a list of all items newer than the given timestamp.
      /// </summary>
      /// <param name="sinceDate">The desired cutoff timestamp.</param>
      /// <returns>A list of items if any exist, otherwise NULL.</returns>
      public IReadOnlyList<ItemRow> GetNewItems(DateTime sinceDate) => GetNewItems(sinceDate, 5000);

      
      public IReadOnlyList<ItemRow> GetNewItems(DateTime sinceDate, int maxItems)
      {
         if (sinceDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("sinceDate must be UTC", nameof(sinceDate));

         if (maxItems <= 0)
            throw new ArgumentException("maxItems must be > 0", nameof(maxItems));

         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            cmd = _dbConn.CreateCommand();

            cmd.CommandText = "SELECT TOP (@count) " + ColumnList +
               " FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id AND i.ins_date > @insDate ORDER BY i.ins_date ASC";

            cmd.Parameters.Add(CreateParameter(cmd, "@count", DbType.Int32, maxItems));
            cmd.Parameters.Add(CreateParameter(cmd, "insDate", DbType.DateTime2, sinceDate));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            var items = ReadToItemList(dr);
            dr.Close();

            if (items == null  ||  items.Count == 0)
               return null;
            else
            {
               // sort by pubDate
               var sorted = from i in items orderby i.PubDate select i;

               return sorted.ToArray();
            }
         }
         finally
         {
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
            }

            cmd?.Dispose();
         }
      }


      /// <summary>
      /// Get the newest item's insertion timestamp.
      /// </summary>
      /// <returns>A UTC timestamp if an item exists in the rss_item table,
      /// otherwise DateTime.MinValue</returns>
      public DateTime GetMaxInsertDate()
      {
         DbCommand cmd = null;

         try
         {
            cmd = _dbConn.CreateCommand();
            cmd.CommandText = "SELECT MAX(ins_date) FROM rss_item";
            object rc = cmd.ExecuteScalar();

            if (rc.GetType().Name == "DBNull")
               return DateTime.MinValue;
            else
               return DateTime.SpecifyKind((DateTime)rc, DateTimeKind.Utc);
         }
         finally
         {
            cmd?.Dispose();
         }
      }


      /// <summary>
      /// Determine whether a URL already exists in the rss_item table.
      /// </summary>
      /// <returns>True if it exists, false otherwise.</returns>
      public bool ItemExists(string url)
      {
         DbCommand cmd = null;

         try
         {
            cmd = _dbConn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM rss_item WHERE url = @url";
            cmd.Parameters.Add(CreateParameter(cmd, "@url", DbType.String, url));

            return ((int)cmd.ExecuteScalar() > 0);
         }
         finally
         {
            cmd?.Dispose();
         }
      }


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
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
               dr = null;
            }

            if (cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
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
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
               dr = null;
            }

            if (cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
         }
      }


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
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
               dr = null;
            }

            if (cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
         }
      }


      /// <summary>
      /// Returns a set of totals which represent the number of items published
      /// by non-regional feeds within the past <paramref name="hours"/>N hours
      /// which contain a state name.
      /// </summary>
      public Dictionary<string, int> GetStateActivityByRange(int hours)
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
            Dictionary<string, int> vals = new Dictionary<string, int>();

            while (dr.Read())
               vals.Add(dr.GetString(0), dr.GetInt32(1));

            return (vals.Count > 0 ? vals : null);
         }
         finally
         {
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
               dr = null;
            }

            if (cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
         }
      }


      /// <summary>
      /// Returns a list of the 100 most frequently occurring words in the title
      /// and description of items retrieved within the last 
      /// <paramref name="hours">N hours</paramref>.
      /// </summary>
      public Dictionary<string, int> GetTrendingWords(int hours)
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
               return GetTrendingWordsLinq(hours);

            cmd.CommandText = @"SELECT TOP 100 CAST(word AS NVARCHAR(60)) [Keyword], COUNT(*) AS [Count] FROM 
rss_item AS i INNER JOIN rss_feed AS f ON 
(f.feed_id = i.feed_id AND f.regional = 0) CROSS APPLY
dbo.UdfSplitString(
   LOWER(i.title) + ' ' + CAST((CAST(LOWER(REPLACE(COALESCE(i.description, ''), '&', '&amp;')) AS XML)).query('/text()') AS NVARCHAR(MAX)),
   1, 1, 1, 1, 1)
WHERE i.ins_date > DATEADD(HOUR, -@hour, GETUTCDATE()) AND word NOT IN (SELECT w.word FROM ignore_word AS w) GROUP BY word ORDER BY COUNT(*) DESC";

            cmd.Parameters.Add(CreateParameter(cmd, "@hour", DbType.Int32, hours));

            Dictionary<string, int> wordCounts = new Dictionary<string, int>();
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            while (dr.Read())
               wordCounts.Add(dr.GetString(0), dr.GetInt32(1));

            return (wordCounts.Count > 0 ? wordCounts : null);
         }
         finally
         {
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
               dr = null;
            }

            if (cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
         }
      }



      /// <summary>
      /// Alternate method of retrieving trending words, using multiple SQL
      /// queries and LINQ.
      /// </summary>
      private Dictionary<string, int> GetTrendingWordsLinq(int hours)
      {
         var ignorableWords = GetIgnorableWords();
         var feeds = GetFeeds();

         var items = GetItemsByRange(
            DateTime.UtcNow.AddHours(-hours), DateTime.UtcNow);

         // filter out items from regional feeds, and split the title/desc
         // text into a set of distinct word arrays
         var splits = items.Where(item => (feeds.Where(f => f.FeedId == item.FeedId).First().IsRegional == false))
            .Select(i => Util.SQLServerStringUtils.SplitString(
               i.Title.ToLower() + " " + i.DescriptionInnerText().ToLower(),
               true, true, true, true, true));

         // flatten the word arrays into a single array, filter out
         // ignorable words, group and select all remaining words by count,
         // sort and take the top 100
         var words = splits.SelectMany(split => split.Cast<string>().ToArray())
            .Where(s => !ignorableWords.Contains(s))
            .GroupBy(g => g)
            .Select(o => new { word = o.Key, count = o.Count() })
            .OrderByDescending(d => d.count)
            .Take(100);

         Dictionary<string, int> wordCounts = new Dictionary<string, int>();

         foreach (var word in words)
            wordCounts.Add(word.word, word.count);

         return (wordCounts.Count > 0 ? wordCounts : null);
      }


      /// <summary>
      /// Retrieve hourly usage counts for a given word, spaning the past 
      /// <paramref name="hours">hours</paramref>. Note that the number of counts
      /// will total N+1 hours, to account for the partial hour which has elapsed
      /// at the time the method is called.
      /// </summary>
      /// <returns>A dictionary in which the keys are date strings of the form
      /// yyyyMMddHH (military hours). and the values are hourly usage counts.</returns>
      public Dictionary<string, int> GetWordHistory(string word, int hours)
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
            cmd.Parameters.Add(CreateParameter(cmd, "@word", DbType.String, word));

            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            DateTime start = DateTime.UtcNow.AddHours(-hours);
            Dictionary<string, int> counts = new Dictionary<string, int>();

            // add an extra data point to the result set to account for the minutes
            // since the clock last chimed a full hour
            for (int h = 0; h < hours + 1; h++)
            {
               counts.Add(start.ToString("yyyyMMddHH"), 0);
               start = start.AddHours(1);
            }

            while (dr.Read())
            {
               DateTime insDate = DateTime.SpecifyKind(dr.GetDateTime(0), DateTimeKind.Utc);
               string dayHour = insDate.ToString("yyyyMMddHH");
               string title = dr.GetString(1).ToLower();
               string desc = dr.GetString(2).ToLower(); ;

               var words = Util.SQLServerStringUtils.SplitString(title + " " + desc,
                  true, true, true, true, true);

               string[] wordArray = words.Cast<string>().ToArray();

               if (!wordArray.Contains(word))
                  continue;

               if (counts.ContainsKey(dayHour))
                  counts[dayHour]++;
            }

            return (counts.Count > 0 ? counts : null);
         }
         finally
         {
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
               dr = null;
            }

            if (cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
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

            return (words.Count > 0 ? words : null);
         }
         finally
         {
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
               dr = null;
            }

            if (cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
         }
      }


      /// <summary>
      /// Successful usage of this method depends on the SQL being executed using
      /// the column list ordering contained in the class-level <c>columnList</c>
      /// attribute.
      /// </summary>
      private static IReadOnlyList<ItemRow> ReadToItemList(DbDataReader dr)
      {
         if (!dr.HasRows)
            return null;

         List<ItemRow> items = new List<ItemRow>();

         while (dr.Read())
         {
            ItemRow item = new ItemRow()
            {
               FeedId = dr.GetInt32(0),
               FeedName = dr.GetString(1),
               InsDate = DateTime.SpecifyKind(dr.GetDateTime(2), DateTimeKind.Utc),
               PubDate = DateTime.SpecifyKind(dr.GetDateTime(3), DateTimeKind.Utc),
               Title = dr.GetString(4),
               Description = !dr.IsDBNull(5) ? dr.GetString(5) : null,
               Url = dr.GetString(6),
               ItemXml = dr.GetString(7)
            };
            
            items.Add(item);
         }

         return items;
      }


      #region demoHelpers
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
            cmd?.Dispose();
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
            cmd?.Dispose();
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
            if (dr != null)
            {
               dr.Close();
               dr.Dispose();
            }
         }
      }
      #endregion


      /// <summary>
      /// Helper method for constructing DBParameters.
      /// </summary>
      private static DbParameter CreateParameter(DbCommand cmd, string name,
         DbType type, object value)
      {
         DbParameter p = cmd.CreateParameter();
         p.ParameterName = name;
         p.DbType = type;
         p.Value = (value  ??  DBNull.Value);

         return p;
      }
   }
}
