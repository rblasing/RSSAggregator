using System;
using System.Web.Mvc;


namespace Portfolio
{
   /// <summary>
   /// http://www.ryadel.com/2014/12/08/http-basic-authentication-asp-net-mvc-using-custom-actionfilter/
   /// and
   /// https://stackoverflow.com/questions/20144364/basic-authentication-in-asp-net-mvc-5/35330494#35330494
   /// </summary>
   public class BasicAuthenticationAttribute : ActionFilterAttribute
   {
      public string BasicRealm { get; set; }
      private string Username { get; }
      private string Password { get; }


      public BasicAuthenticationAttribute(string username, string password)
      {
         this.Username = username;
         this.Password = password;
      }


      public override void OnActionExecuting(ActionExecutingContext filterContext)
      {
         var req = filterContext.HttpContext.Request;
         var auth = req.Headers["Authorization"];

         if (!String.IsNullOrEmpty(auth))
         {
            var cred = System.Text.Encoding.ASCII.GetString(
               Convert.FromBase64String(auth.Substring(6))).Split(':');

            var user = new { Name = cred[0], Pass = cred[1] };

            if (user.Name == Username  &&  user.Pass == Password)
               return;
         }

         filterContext.HttpContext.Response.AddHeader(
            "WWW-Authenticate", String.Format("Basic realm=\"{0}\"", BasicRealm ?? "Ryadel"));

         filterContext.Result = new HttpUnauthorizedResult();
      }
   }
}