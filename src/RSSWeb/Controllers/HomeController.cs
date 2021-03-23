// Copyright 2020 Richard Blasingame. All rights reserved.

using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using RSS;


namespace Portfolio.Controllers
{
   public class HomeController : Controller
   {
      private static readonly string ConnStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
      private static readonly int MaxItems = int.Parse((ConfigurationManager.AppSettings["viewMaxItems"]));
      private static readonly string SseUri = ConfigurationManager.AppSettings["sseUri"];
      private static readonly string XslUri = ConfigurationManager.AppSettings["xslUri"];

      private static readonly XslCompiledTransform Xsl;


      static HomeController()
      {
         // only load file from disk once to improve performance
         if (Xsl == null)
         {
            Xsl = new XslCompiledTransform(true);
            Xsl.Load(System.Web.HttpContext.Current.Server.MapPath(XslUri));
         }
      }


      /// <summary>
      /// When the user first loads the page, get the latest <c>maxItems</c>
      /// items from the database, transform the XML to HTML, and send that back
      /// to the client as the initial view.
      /// </summary>
      public ActionResult Index()
      {
         SqlConnection dbConn = null;

         try
         {
            dbConn = new SqlConnection(ConnStr);
            dbConn.Open();
            Dal dal = new Dal(dbConn);
            ItemRow[] topItems = dal.GetTopItems(MaxItems);
            StringBuilder sb = new StringBuilder();

            if (topItems != null)
            {
               foreach (ItemRow item in topItems)
                  sb.Append(TransForm(item.ToXml(false)));
            }

            ViewBag.topItems = sb.ToString();
            ViewBag.maxItems = MaxItems;
            ViewBag.sseUri = SseUri;

            return View();
         }
         finally
         {
            if (dbConn != null)
            {
               if (dbConn.State != System.Data.ConnectionState.Closed)
                  dbConn.Close();

               dbConn.Dispose();
            }
         }
      }


      private static string TransForm(string xml)
      {
         string transformedXml;

         using (StringReader sr = new StringReader(xml))
         {
            XPathDocument xPath = new XPathDocument(sr);

            using (TextWriter txtWriter = new StringWriter(new StringBuilder()))
            {
               XmlWriter oWriter = new XmlTextWriter(txtWriter);
               Xsl.Transform(xPath, null, oWriter);
               transformedXml = txtWriter.ToString();
            }
         }

         return transformedXml;
      }
   }
}