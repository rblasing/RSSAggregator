using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using RSS;


namespace RSSWeb.Controllers
{
   public class HomeController : ControllerBase
   {
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
            var topItems = dal.GetTopItems(MaxItems);
            StringBuilder sb = new StringBuilder();

            if (topItems != null)
            {
               // these characters will prevent Html.Raw() from parsing the
               // entire string in the client code
               foreach (ItemRow item in topItems)
                  sb.Append(TransForm(item.ToXml(false)).Replace("'", "&apos;").Replace("\r", "").Replace("\n", ""));
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


      /// <summary>
      /// This transforms the XML representation of an Item object to an HTML
      /// div containing the same information.  This is incredibly tight
      /// coupling to the view, but it does serve the purpose of illustrating
      /// usage of a XSL transformation.
      /// </summary>
      /// <param name="xml"></param>
      /// <returns></returns>
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