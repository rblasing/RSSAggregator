// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;


namespace RSS
{
   public class DAL : IDAL
   {
      private readonly DbConnection dbConn;

      private readonly string ColumnList =
         "i.feed_id, f.title, i.ins_date, i.pub_date, i.title, i.description, i.url, i.xml";

      private static List<string> StateNames = null;


      public DAL(DbConnection inConn)
      {
         dbConn = inConn;

         // load once on app/site initialization
         if (StateNames == null)
         {
            DbCommand cmd = null;
            DbDataReader dr = null;

            try
            {
               if (dbConn.State != ConnectionState.Open)
                  dbConn.Open();
   
               cmd = dbConn.CreateCommand();
               cmd.CommandText = "SELECT name FROM state";
               dr = cmd.ExecuteReader(CommandBehavior.SingleResult);
               StateNames = new List<string>();

               while (dr.Read())
                  StateNames.Add(dr.GetString(0));
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
      public bool insertFeed(string title, string url, bool isRegional)
      {
         DbCommand cmd = null;

         try
         {
            cmd = dbConn.CreateCommand();
            cmd.CommandText = "INSERT INTO rss_feed (title, url, regional, active) VALUES (@title, @url, @regional, 1)";
            cmd.Parameters.Add(createParameter(cmd, "@title", DbType.String, title));
            cmd.Parameters.Add(createParameter(cmd, "@url", DbType.String, url));
            cmd.Parameters.Add(createParameter(cmd, "@regional", DbType.Boolean, isRegional));

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
      public bool deleteFeed(int feedId)
      {
         DbCommand cmd = null;
         DbTransaction t = null;

         try
         {
            t = dbConn.BeginTransaction();

            cmd = dbConn.CreateCommand();
            cmd.CommandText = "DELETE FROM rss_item WHERE feed_id = @id";
            cmd.Parameters.Add(createParameter(cmd, "@id", DbType.Int32, feedId));
            cmd.Transaction = t;

            // there might not be any associated items, so don't rollback if
            // the return code is < 1
            cmd.ExecuteNonQuery();

            cmd = dbConn.CreateCommand();
            cmd.CommandText = "DELETE FROM rss_feed WHERE feed_id = @id";
            cmd.Parameters.Add(createParameter(cmd, "@id", DbType.Int32, feedId));
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


      public bool editFeed(int feedId, string title, string url, bool isRegional,
         bool isActive)
      {
         DbCommand cmd = null;

         try
         {
            cmd = dbConn.CreateCommand();
            cmd.CommandText = "UPDATE rss_feed SET title = @title, url = @url, regional = @regional, active = @active WHERE feed_id = @id";
            cmd.Parameters.Add(createParameter(cmd, "@title", DbType.String, title));
            cmd.Parameters.Add(createParameter(cmd, "@url", DbType.String, url));
            cmd.Parameters.Add(createParameter(cmd, "@regional", DbType.Boolean, isRegional));
            cmd.Parameters.Add(createParameter(cmd, "@active", DbType.Boolean, isActive));
            cmd.Parameters.Add(createParameter(cmd, "@id", DbType.Int32, feedId));

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
      public RSS.FeedRow[] getFeeds()
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            List<FeedRow> items = new List<FeedRow>();

            cmd = dbConn.CreateCommand();
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

            return items.ToArray();
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
      public bool insertItem(int feedId, Item item)
      {
         if (item.pubDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("pubDate must be UTC", "item.pubDate");

         DbCommand cmd = null;
         DbTransaction t = null;

         try
         {
            DateTime maxDate = getMaxInsertDate();
            DateTime insDate = DateTime.UtcNow;

            while (insDate <= maxDate)
               insDate = DateTime.UtcNow;

            t = dbConn.BeginTransaction();
            cmd = dbConn.CreateCommand();
            cmd.Transaction = t;

            cmd.CommandText =
               "INSERT INTO rss_item (feed_id, ins_date, pub_date, title, description, url, xml) VALUES " +
               "(@feedid, @insDate, @pubdate, @title, @desc, @url, @xml)";

            cmd.Parameters.Add(createParameter(cmd, "@feedid", DbType.Int32, feedId));
            cmd.Parameters.Add(createParameter(cmd, "@insdate", DbType.DateTime2, insDate));
            cmd.Parameters.Add(createParameter(cmd, "@pubdate", DbType.DateTime2, item.pubDate));
            cmd.Parameters.Add(createParameter(cmd, "@title", DbType.String, item.title.Trim()));
            cmd.Parameters.Add(createParameter(cmd, "@desc", DbType.String, ("" + item.description).Trim()));
            cmd.Parameters.Add(createParameter(cmd, "@url", DbType.String, item.url));
            cmd.Parameters.Add(createParameter(cmd, "@xml", DbType.Xml, item.itemXml));

            int rc = cmd.ExecuteNonQuery();

            if (rc != 1)
               return false;

            // increment the state table's grand totals, if  one of more state names
            // are referenced in the current item
            foreach (string name in StateNames)
            {
               if (item.title.ToUpper().Contains(name)  ||
                  ("" + item.description).ToUpper().Contains(name))
               {
                  cmd.CommandText = "UPDATE state SET ref_count = ref_count + 1 WHERE " +
                     "name = @name AND " +
                     "EXISTS (SELECT regional FROM rss_feed WHERE feed_id = @id AND regional = 0)";

                  cmd.Parameters.Clear();
                  cmd.Parameters.Add(createParameter(cmd, "@name", DbType.String, name));
                  cmd.Parameters.Add(createParameter(cmd, "@id", DbType.Int32, feedId));
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
      /// <returns>Array of items if any exist, otherwise NULL.</returns>
      public ItemRow[] getTopItems(int itemCount)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (itemCount <= 0)
            throw new ArgumentException("itemCount must be > 0", "itemCount");

         try
         {
            cmd = dbConn.CreateCommand();

            cmd.CommandText = "SELECT TOP (@count) " + ColumnList +
               " FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id ORDER BY i.ins_date DESC";

            cmd.Parameters.Add(createParameter(cmd, "@count", DbType.Int32, itemCount));

            cmd.CommandTimeout = 100000;
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            ItemRow[] items = readToArray(dr);
            dr.Close();

            if (items == null)
               return null;
            else
            {
               // sort by pubDate
               var sorted = from i in items orderby i.pubDate descending select i;

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
      /// <returns>An array of items if any exist, otherwise NULL.</returns>
      public ItemRow[] getNewItems(DateTime sinceDate)
      {
         return getNewItems(sinceDate, 5000);
      }

      
      public ItemRow[] getNewItems(DateTime sinceDate, int maxItems)
      {
         if (sinceDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("sinceDate must be UTC", "sinceDate");

         if (maxItems <= 0)
            throw new ArgumentException("maxItems must be > 0", "maxItems");

         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            cmd = dbConn.CreateCommand();

            cmd.CommandText = "SELECT TOP (@count) " + ColumnList +
               " FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id AND i.ins_date > @insDate ORDER BY i.ins_date ASC";

            cmd.Parameters.Add(createParameter(cmd, "@count", DbType.Int32, maxItems));
            cmd.Parameters.Add(createParameter(cmd, "insDate", DbType.DateTime2, sinceDate));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            ItemRow[] items = readToArray(dr);
            dr.Close();

            if (items == null)
               return null;
            else
            {
               // sort by pubDate
               var sorted = from i in items orderby i.pubDate select i;

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
      public DateTime getMaxInsertDate()
      {
         DbCommand cmd = null;

         try
         {
            cmd = dbConn.CreateCommand();
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
      /// Determine whether a new item's URL already exists in the rss_item table.
      /// </summary>
      /// <param name="url">The new item's URL</param>
      /// <returns>True if it exists, false otherwise.</returns>
      public bool itemExists(string url)
      {
         DbCommand cmd = null;

         try
         {
            cmd = dbConn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM rss_item WHERE url = @url";
            cmd.Parameters.Add(createParameter(cmd, "@url", DbType.String, url));

            return ((int)cmd.ExecuteScalar() > 0);
         }
         finally
         {
            cmd?.Dispose();
         }
      }


      /// <summary>
      /// Retrieve a set of items whose titles contain one or more of the
      /// specified space-separated keywords.
      /// </summary>
      public ItemRow[] getItemsByKeywords(string keywords)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            cmd = dbConn.CreateCommand();

            // use case-insensitive collation for LIKE
            cmd.CommandText = "SELECT " + ColumnList +
               " FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id AND EXISTS " +
               "(SELECT value FROM STRING_SPLIT(@keywords, ' ') WHERE TRIM(value) != '' AND i.title COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + value + '%')";

            cmd.Parameters.Add(createParameter(cmd, "@keywords", DbType.String, keywords.ToLower()));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            ItemRow[] items = readToArray(dr);
            dr.Close();

            return items;
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
      public ItemRow[] getItemsByRange(DateTime lowDate, DateTime highDate)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (lowDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("lowDate must be UTC", "lowDate");

         if (highDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("highDate must be UTC", "highDate");

         if (highDate <= lowDate)
            throw new ArgumentException("highDate must be greater than lowDate");

         try
         {
            cmd = dbConn.CreateCommand();
            
            cmd.CommandText = "SELECT " + ColumnList +
               " FROM rss_item i, rss_feed f WHERE i.feed_id = f.feed_id AND i.pub_date >= @lowDate AND i.pub_date <= @highDate ORDER BY i.ins_date";

            cmd.Parameters.Add(createParameter(cmd, "@lowDate", DbType.DateTime2, lowDate));
            cmd.Parameters.Add(createParameter(cmd, "@highDate", DbType.DateTime2, highDate));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            ItemRow[] items = readToArray(dr);
            dr.Close();

            return items;
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
      /// given state's name.
      /// </summary>
      public ItemRow[] getItemsByState(string abbr, int count)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (string.IsNullOrEmpty(abbr))
            throw new ArgumentException("abbr is a required parameter", "abbr");

         if (count <= 0)
            throw new ArgumentException("count must be > 0", "count");

         try
         {
            cmd = dbConn.CreateCommand();

            cmd.CommandText = "SELECT TOP (@count) " + ColumnList +
               " FROM rss_item i, rss_feed f, state s WHERE s.abbr = @abbr AND " +
               "i.feed_id = f.feed_id AND f.regional = 0 AND " +
               "(i.title COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + s.name + '%' OR i.description COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + s.name + '%') " +
               "ORDER BY i.ins_date DESC";

            cmd.Parameters.Add(createParameter(cmd, "@count", DbType.Int32, count));
            cmd.Parameters.Add(createParameter(cmd, "@abbr", DbType.String, abbr));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            ItemRow[] items = readToArray(dr);
            dr.Close();

            return items;
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
      public Dictionary<string, int> getStateActivityByRange(int hours)
      {
         DbCommand cmd = null;
         DbDataReader dr = null;

         if (hours <= 0)
            throw new ArgumentException("hours must be > 0", "hours");

         try
         {
            cmd = dbConn.CreateCommand();

            cmd.CommandText = "WITH tbl AS (SELECT i.title, i.description FROM rss_item i, rss_feed f WHERE f.feed_id = i.feed_id and f.regional = 0 AND i.ins_date > DATEADD(HOUR, @hour, GETUTCDATE())) " +
               "SELECT s.abbr, COUNT(*) FROM tbl, state s WHERE tbl.title COLLATE sql_latin1_general_cp1_ci_as LIKE '% + s.name + %' OR tbl.description COLLATE sql_latin1_general_cp1_ci_as LIKE '%' + s.name + '%' " +
               "GROUP BY s.abbr ORDER BY COUNT(*) DESC, s.abbr ASC";

            cmd.Parameters.Add(createParameter(cmd, "@hour", DbType.Int32, -hours));
            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);
            Dictionary<string, int> vals = new Dictionary<string, int>();

            while (dr.Read())
               vals.Add(dr.GetString(0), dr.GetInt32(1));

            return vals;
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
      private ItemRow[] readToArray(DbDataReader dr)
      {
         if (!dr.HasRows)
            return null;

         List<ItemRow> items = new List<ItemRow>();

         while (dr.Read())
         {
            ItemRow item = new ItemRow();
            item.feedId = dr.GetInt32(0);
            item.feedName = dr.GetString(1);
            item.insDate = DateTime.SpecifyKind(dr.GetDateTime(2), DateTimeKind.Utc);
            item.pubDate = DateTime.SpecifyKind(dr.GetDateTime(3), DateTimeKind.Utc);
            item.title = dr.GetString(4);
            item.description = !dr.IsDBNull(5) ? dr.GetString(5) : null;
            item.url = dr.GetString(6);
            item.itemXml = dr.GetString(7);

            items.Add(item);
         }

         return items.ToArray();
      }


      #region demoHelpers
      /// <summary>
      /// Execute an arbitrary SQL SELECT statement.
      /// 
      /// Um, don't put something like this in a real app, m'kay?
      /// </summary>
      public DataTable select(string sql)
      {
         DbCommand cmd = null;

         try
         {
            cmd = dbConn.CreateCommand();
            cmd.CommandText = sql;

            return selectToDataTable(cmd);
         }
         finally
         {
            cmd?.Dispose();
         }
      }


      public DataTable selectByKeyword(string keyword)
      {
         DbCommand cmd = null;

         try
         {
            cmd = dbConn.CreateCommand();
            cmd.CommandText = "SELECT title [Title] FROM rss_item WHERE title COLLATE sql_latin1_general_cp1_ci_as LIKE @tk OR xml.exist('/item/description/text()[contains(lower-case(.), sql:variable(\"@dk\"))]') = 1";
            cmd.Parameters.Add(createParameter(cmd, "@tk", DbType.String, "%" + keyword.ToLower() + "%"));
            cmd.Parameters.Add(createParameter(cmd, "@dk", DbType.String, keyword.ToLower()));

            return selectToDataTable(cmd);
         }
         finally
         {
            cmd?.Dispose();
         }
      }


      private DataTable selectToDataTable(DbCommand cmd)
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
      private DbParameter createParameter(DbCommand cmd, string name,
         DbType type, object value)
      {
         DbParameter p = cmd.CreateParameter();
         p.ParameterName = name;
         p.DbType = type;
         p.Value = (value == null ? DBNull.Value : value);

         return p;
      }
   }
}
