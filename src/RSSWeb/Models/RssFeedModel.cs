// Copyright 2020 Richard Blasingame. All rights reserved.

using System.ComponentModel.DataAnnotations;


namespace Portfolio.Models
{
   public class RssFeedModel
   {
      [Display(Name = "Feed ID")]
      public int FeedId { get; set; }

      [Display(Name = "Title")]
      [Required(ErrorMessage = "Title is required.")]
      public string Title { get; set; }

      [Display(Name = "URL")]
      [Required(ErrorMessage = "URL is required.")]
      public string Url { get; set; }

      [Display(Name = "Active?")]
      [Required(ErrorMessage = "Active Y/N is required.")]
      public bool Active { get; set; }

      [Display(Name = "Regional?")]
      [Required(ErrorMessage = "Regional Y/N is required.")]
      public bool Regional { get; set; }


      public RssFeedModel()
      {
      }


      public RssFeedModel(int i, string t, string u, bool r, bool a)
      {
         FeedId = i;
         Title = t;
         Url = u;
         Regional = r;
         Active = a;
      }
   }
}