using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using RSS;


namespace RSSWeb.Models
{
   public class LogModel
   {
      [Display(Name = "Page Number")]
      [Required(ErrorMessage = "PageNumber is required.")]
      public int PageNumber { get; set; }

      [Display(Name = "Page Count")]
      [Required(ErrorMessage = "PageCount is required.")]
      public int PageCount { get; set; }

      [Display(Name = "Log Entries")]
      [Required(ErrorMessage = "LogEntries is required.")]
      public List<LogEntry> LogEntries { get; set; }


      public LogModel()
      {
      }


      public LogModel(int pageNum, int pageCount, List<LogEntry> entries)
      {
         PageNumber = pageNum;
         PageCount = pageCount;
         LogEntries = entries;
      }
   }
}