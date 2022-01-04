using System;
using System.Data;
using System.Data.Common;


namespace RSSWeb
{
   public class TablePager
   {
      private string _sql;
      private string _countSql;
      private int _pageSize;
      private int _numPages;
      private int _currentPage;
      private int _rowCount;

      public int NumPages { get => _numPages; }
      public int CurrentPage { get => _currentPage; }


      public TablePager(DbConnection conn, string countSql, string sql, int pageSize)
      {
         _sql = sql;
         _countSql = countSql;
         _pageSize = pageSize;

         if (!countSql.ToLower().Contains("count("))
            throw new ArgumentException("countSql must select a COUNT value");

         if (!sql.ToLower().Contains("order by"))
            throw new ArgumentException("sql must contain an ORDER BY clause");

         // determine how many rows are in the resultset so that number of
         // pages many be calculated
         DbCommand cmd = null;

         try
         {
            if (conn.State != ConnectionState.Open)
               conn.Open();

            cmd = conn.CreateCommand();
            cmd.CommandText = countSql;
            _rowCount = (int)cmd.ExecuteScalar();
         }
         finally
         {
            if (cmd != null)
            {
               cmd.Dispose();
               cmd = null;
            }
         }

         _numPages = (int)Math.Ceiling((double)_rowCount / pageSize);
      }


      /// <summary>
      /// Returns a page of data, containing the number of rows per page
      /// specified in the constructor.
      /// </summary>
      /// <param name="page">0-indexed page of data requested</param>
      /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="page"/>
      /// is larger than the number of pages - 1.</exception>
      public DataTable GetPage(DbConnection conn, int page)
      {
         if (page >= _numPages)
            throw new IndexOutOfRangeException("page argument is larger than numPages");

         int offSet = page * _pageSize;
         _currentPage = page;

         if (conn.State != ConnectionState.Open)
            conn.Open();

         DbCommand cmd = null;
         DbDataReader dr = null;

         try
         {
            cmd = conn.CreateCommand();

            cmd.CommandText =
               $"{ _sql} OFFSET {offSet.ToString()} ROWS FETCH NEXT {_pageSize.ToString()} ROWS ONLY";

            dr = cmd.ExecuteReader(CommandBehavior.SingleResult);

            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();

            if (dt.Rows.Count == 0)
            {
               /* I considered silently looping around here, but in the long
               run I think it better to encourage the caller to enforce edge
               boundaries.
               cmd.CommandText = _countSql;
               _rowCount = (int)cmd.ExecuteScalar();
               _numPages = (int)Math.Ceiling((double)_rowCount / _pageSize);
               _currentPage = 0; */

               throw new IndexOutOfRangeException(
                  "No rows returned with numPages argument " + _numPages);
            }

            return dt;
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
}
