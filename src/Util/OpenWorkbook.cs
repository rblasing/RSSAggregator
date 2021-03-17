using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace Util
{
   public static class OpenWorkbook
   {
      public static bool WriteOpenXMLWorkbook(string filename, MemoryStream ms,
         System.Data.Common.DbDataReader dr, string[] columnHeaders)
      {
         // header row is 1
         int rowIdx = 2;

         SpreadsheetDocument xlsx = null;

         try
         {
            if (ms != null)
               xlsx = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            else
               xlsx = SpreadsheetDocument.Create(filename, SpreadsheetDocumentType.Workbook);

            xlsx.AddWorkbookPart();
            WorksheetPart sheet = xlsx.WorkbookPart.AddNewPart<WorksheetPart>();

            OpenXmlWriter oxw = OpenXmlWriter.Create(sheet);

            // start worksheet
            oxw.WriteStartElement(new Worksheet());

            // start sheetdata
            oxw.WriteStartElement(new SheetData());

            OpenWorkbook.AddOpenXMLHeader(oxw, columnHeaders);

            while (dr.Read())
            {
               List<OpenXmlAttribute> oxa = new List<OpenXmlAttribute>();

               // row index
               oxa.Add(new OpenXmlAttribute("r", null, rowIdx.ToString()));

               // start row
               oxw.WriteStartElement(new Row(), oxa);

               for (int idx = 0; idx < dr.FieldCount; idx++)
                  OpenWorkbook.AddOpenXMLCell(oxw, GetStringOrNull(dr, idx));

               // end row
               oxw.WriteEndElement();

               rowIdx++;
            }

            // end sheetData
            oxw.WriteEndElement();

            // end worksheet
            oxw.WriteEndElement();
            oxw.Close();

            oxw = OpenXmlWriter.Create(xlsx.WorkbookPart);
            oxw.WriteStartElement(new Workbook());
            oxw.WriteStartElement(new Sheets());

            oxw.WriteElement(new Sheet()
            {
               Name = "Sheet1",
               SheetId = 1,
               Id = xlsx.WorkbookPart.GetIdOfPart(sheet)
            });

            // end Sheets
            oxw.WriteEndElement();

            // end Workbook
            oxw.WriteEndElement();

            oxw.Close();
            xlsx.Close();

            return true;
         }
         finally
         {
            if (xlsx != null)
            {
               xlsx.Dispose();
               xlsx = null;
            }
         }
      }


      private static void AddOpenXMLHeaderCell(OpenXmlWriter writer, string val)
      {
         var oxa = new List<OpenXmlAttribute>();

         // this is the data type ("t"), with CellValues.String ("str")
         oxa.Add(new OpenXmlAttribute("t", null, "str"));

         // start cell
         writer.WriteStartElement(new Cell() { StyleIndex = (UInt32)1 }, oxa);
         writer.WriteElement(new CellValue(val));

         // end cell
         writer.WriteEndElement();
      }


      private static void AddOpenXMLHeader(OpenXmlWriter oxw, string[] headerTitles)
      {
         List<OpenXmlAttribute> oxa = new List<OpenXmlAttribute>();
         oxa.Add(new OpenXmlAttribute("r", null, "1"));

         // start row
         oxw.WriteStartElement(new Row(), oxa);

         foreach (string s in headerTitles)
            AddOpenXMLHeaderCell(oxw, s);

         // end row
         oxw.WriteEndElement();
      }


      private static void AddOpenXMLCell(OpenXmlWriter writer, string val)
      {
         var oxa = new List<OpenXmlAttribute>();
         oxa.Add(new OpenXmlAttribute("t", null, "str"));

         // start cell
         writer.WriteStartElement(new Cell(), oxa);
         Cell c = new Cell();
         writer.WriteElement(new CellValue(val));

         // end cell
         writer.WriteEndElement();
      }

      
      private static string GetStringOrNull(DbDataReader dr, int idx)
      {
         if (dr == null  ||  dr.IsDBNull(idx))
            return null;
         else
            return dr[idx].ToString().Trim();
      }
   }
}
