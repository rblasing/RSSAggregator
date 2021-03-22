// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;


namespace Portfolio.Models
{
   public class RSSFeedModel
   {
      [Display(Name = "Feed ID")]
      public int feed_id { get; set; }

      [Display(Name = "Title")]
      [Required(ErrorMessage = "Title is required.")]
      public string title { get; set; }

      [Display(Name = "URL")]
      [Required(ErrorMessage = "URL is required.")]
      public string url { get; set; }

      [Display(Name = "Active?")]
      [Required(ErrorMessage = "Active Y/N is required.")]
      public bool active { get; set; }

      [Display(Name = "Regional?")]
      [Required(ErrorMessage = "Regional Y/N is required.")]
      public bool regional { get; set; }


      public RSSFeedModel()
      {
      }


      public RSSFeedModel(int i, string t, string u, bool r, bool a)
      {
         feed_id = i;
         title = t;
         url = u;
         regional = r;
         active = a;
      }
   }
}