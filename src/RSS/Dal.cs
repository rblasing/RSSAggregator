using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;


namespace RSS
{
   public class Dal : IDal
   {
      protected readonly DbConnection _dbConn;

      protected readonly string ColumnList =
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
               Cleanup(cmd, dr);
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
            Cleanup(cmd, null);
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
            t?.Rollback();
            t?.Dispose();
            t = null;

            Cleanup(cmd, null);
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
            Cleanup(cmd, null);
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
            Cleanup(cmd, dr);
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

            // increment the state table's grand totals, if one of more state names
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
            t?.Rollback();
            t?.Dispose();
            t = null;

            Cleanup(cmd, null);
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
            Cleanup(cmd, dr);
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
            Cleanup(cmd, dr);
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
            Cleanup(cmd, null);
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
            Cleanup(cmd, null);
         }
      }


      /// <summary>
      /// Successful usage of this method depends on the SQL being executed using
      /// the column list ordering contained in the class-level <c>columnList</c>
      /// attribute.
      /// </summary>
      protected static IReadOnlyList<ItemRow> ReadToItemList(DbDataReader dr)
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


      /// <summary>
      /// Helper method for constructing DBParameters.
      /// </summary>
      protected static DbParameter CreateParameter(DbCommand cmd, string name,
         DbType type, object value)
      {
         DbParameter p = cmd.CreateParameter();
         p.ParameterName = name;
         p.DbType = type;
         p.Value = (value  ??  DBNull.Value);

         return p;
      }


      public static void Cleanup(DbCommand cmd, DbDataReader dr)
      {
         dr?.Close();
         dr?.Dispose();
         
         if (cmd != null)
            cmd.Connection = null;

         cmd?.Dispose();
      }
   }
}
