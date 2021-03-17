// Copyright 2020 Richard Blasingame. All rights reserved.

using System;
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
      private static readonly string connStr = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;
      private static readonly int maxItems = int.Parse((ConfigurationManager.AppSettings["viewMaxItems"]));
      private static readonly string sseUri = ConfigurationManager.AppSettings["sseUri"];
      private static readonly string xslUri = ConfigurationManager.AppSettings["xslUri"];

      private static readonly XslCompiledTransform xsl = null;


      static HomeController()
      {
         // only load file from disk once to improve performance
         if (xsl == null)
         {
            xsl = new XslCompiledTransform(true);
            xsl.Load(System.Web.HttpContext.Current.Server.MapPath(xslUri));
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
            dbConn = new SqlConnection(connStr);
            dbConn.Open();
            DAL dal = new DAL(dbConn);
            ItemRow[] topItems = dal.getTopItems(maxItems);
            StringBuilder sb = new StringBuilder();

            if (topItems != null)
            {
               foreach (ItemRow item in topItems)
                  sb.Append(TransForm(item.toXml(false)));
            }

            ViewBag.topItems = sb.ToString();
            ViewBag.maxItems = maxItems;
            ViewBag.sseUri = sseUri;

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
         string transformedXml = string.Empty;

         using (StringReader sr = new StringReader(xml))
         {
            XPathDocument xPath = new XPathDocument(sr);

            using (TextWriter txtWriter = new StringWriter(new StringBuilder()))
            {
               XmlWriter oWriter = new XmlTextWriter(txtWriter);
               xsl.Transform(xPath, null, oWriter);
               transformedXml = txtWriter.ToString();
            }
         }

         return transformedXml;
      }
   }
}