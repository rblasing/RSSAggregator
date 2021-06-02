using System;
using System.Configuration;
using System.Web.Mvc;


namespace RSSWeb.Controllers
{
   public class ControllerBase : Controller
   {
      protected static readonly string SseUri = ConfigurationManager.AppSettings["sseUri"];

      protected static readonly int HeatMapHours =
         int.Parse((ConfigurationManager.AppSettings["heatMapHours"]));

      protected static readonly string ConnStr =
         ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;

      protected static readonly int MaxItems =
         int.Parse((ConfigurationManager.AppSettings["viewMaxItems"]));

      protected static readonly string XslUri = ConfigurationManager.AppSettings["xslUri"];
   }
}