using System;


namespace RSS
{
   // fields used in a Log4Net log entry
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   public class LogEntry
   {
      public int id;
      public DateTime date;
      public string thread;
      public string level;
      public string logger;
      public string message;
      public string exception;
   }
}
