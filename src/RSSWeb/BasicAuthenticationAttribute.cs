using System;
using System.Configuration;
using System.Web.Mvc;


namespace RSSWeb
{
   /// <summary>
   /// http://www.ryadel.com/2014/12/08/http-basic-authentication-asp-net-mvc-using-custom-actionfilter/
   /// and
   /// https://stackoverflow.com/questions/20144364/basic-authentication-in-asp-net-mvc-5/35330494#35330494
   /// 
   /// I removed the user/password attributes from the constructor (as in the
   /// articles) and added them to web.config.
   /// </summary>
   public sealed class BasicAuthenticationAttribute : ActionFilterAttribute
   {
      public string BasicRealm { get; set; }


      public override void OnActionExecuting(ActionExecutingContext filterContext)
      {
         string adminId  = ConfigurationManager.AppSettings["adminId"];
         string adminPassword = ConfigurationManager.AppSettings["adminPassword"];

         var req = filterContext.HttpContext.Request;
         var auth = req.Headers["Authorization"];

         if (!String.IsNullOrEmpty(auth))
         {
            var parts = System.Text.Encoding.ASCII.GetString(
               Convert.FromBase64String(auth.Substring(6))).Split(':');

            var user = new { Name = parts[0], Pass = parts[1] };

            if (user.Name == adminId  &&  user.Pass == adminPassword)
               return;
         }

         filterContext.HttpContext.Response.AddHeader(
            "WWW-Authenticate", String.Format("Basic realm=\"{0}\"", BasicRealm ?? "RSSWeb"));

         filterContext.Result = new HttpUnauthorizedResult();
      }
   }
}