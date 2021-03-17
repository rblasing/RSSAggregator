using System;


namespace Util
{
   public static class JS
   {
      // create DateTime from JavaScript ticks
      public static DateTime DateFromJSTicks(long ticks)
      {
         return new DateTime(ticks * 10000 + 621355968000000000, DateTimeKind.Utc);
      }


      public static long JSTicksFromDate(DateTime d)
      {
         return ((d.Ticks - 621355968000000000) / 10000);
      }
   }
}
