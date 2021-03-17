// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
using System.Web;


namespace RSSWCFSvc
{
   public class Global : HttpApplication
   {
      protected void Application_BeginRequest(object sender, EventArgs e)
      {
         HttpApplication app = null;

         try
         {
            // add a trace filter to the output stream so it can be logged
            app = sender as HttpApplication;
            app.Response.Filter = new Util.FilterStream(app.Response.Filter);

            // log the input request
            byte[] bytes = new byte[app.Request.InputStream.Length];
            app.Request.InputStream.Read(bytes, 0, (int)app.Request.InputStream.Length);

            String s = System.Text.Encoding.UTF8.GetString(bytes);

            if (!string.IsNullOrWhiteSpace(s))
               log4net.LogManager.GetLogger("GlobalRequest").Debug(s);
         }
         catch (Exception )
         {
            // logging is a nice-to-have feature - its failure should never 
            // crash the service
         }
         finally
         {
            // don't forget to reset the stream
            if (app != null  &&  app.Request != null  &&  app.Request.InputStream != null)
               app.Request.InputStream.Position = 0;
         }
      }


      void Application_EndRequest(Object sender, EventArgs e)
      {
         try
         {
            // log the response message that was sent to the client
            HttpApplication app = sender as HttpApplication;
            string s = ((Util.FilterStream)app.Response.Filter).ReadStream();

            if (!string.IsNullOrWhiteSpace(s))
               log4net.LogManager.GetLogger("GlobalResponse").Debug(s);
         }
         catch (Exception)
         {
         }
      }
   }
}