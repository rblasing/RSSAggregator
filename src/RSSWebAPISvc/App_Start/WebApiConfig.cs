using System;
using System.Web.Http;


[assembly: log4net.Config.XmlConfigurator(Watch = true)]


namespace RSSWebAPISvc
{
   public static class WebApiConfig
   {
      public static void Register(HttpConfiguration config)
      {
         // this is required to force the serializer to append milliseconds
         // of .0000000 to ISO datetime representations, in cases where the
         // time value has no significant milliseconds
         config.Formatters.JsonFormatter.SerializerSettings.DateFormatString = "o";

         config.EnableSystemDiagnosticsTracing();

         // Web API routes
         config.MapHttpAttributeRoutes();

         config.Routes.MapHttpRoute(
            name: "ActionApi",
            routeTemplate: "api/{controller}/{action}/{id1}/{id2}",
            defaults: new { id1 = RouteParameter.Optional, id2 = RouteParameter.Optional }
         );
      }
   }
}