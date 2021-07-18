using System;
using System.IO;
using System.Web;


namespace RSSWeb
{
   /// <summary>
   /// Listens for client POSTs containing error information meant to be logged
   /// for debugging purposes.  The request body will contain the error details,
   /// formatted in JSON notation:
   /// {
   ///    message: string,
   ///    url: string,
   ///    line: number,
   ///    column: number,
   ///    error: string
   /// }
   /// </summary>
   public class ClientErrorHandler : IHttpHandler
   {
      public void ProcessRequest(HttpContext context)
      {
         StreamReader sr = null;
         context.Response.ContentType = "text/plain";

         try
         {
            sr = new StreamReader(context.Request.InputStream);
            string body = sr.ReadToEnd();
            log4net.LogManager.GetLogger("ClientErrorHandler").Error(body);
            context.Response.Write("OK");
         }
         catch (Exception e)
         {
            log4net.LogManager.GetLogger("ClientErrorHandler").Error(e.Message);
            context.Response.Write("Failed");
         }
         finally
         {
            sr?.Dispose();
         }

         context.Response.Flush();
      }


      public bool IsReusable => false;
   }
}