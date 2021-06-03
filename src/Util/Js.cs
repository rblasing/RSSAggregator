using System;


namespace Util
{
   public static class Js
   {
      // create DateTime from JavaScript ticks
      public static DateTime DateFromJsTicks(long ticks)
      {
         return new DateTime(ticks * 10000 + 621355968000000000, DateTimeKind.Utc);
      }


      public static long JsTicksFromDate(DateTime d)
      {
         return ((d.Ticks - 621355968000000000) / 10000);
      }
   }
}
