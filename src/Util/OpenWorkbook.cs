using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace Util
{
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   public static class OpenWorkbook
   {
      public static bool WriteOpenXmlWorkbook(string filename, MemoryStream ms,
         DbDataReader dr, string[] columnHeaders)
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

            OpenWorkbook.AddOpenXmlHeader(oxw, columnHeaders);

            while (dr.Read())
            {
               List<OpenXmlAttribute> oxa = new List<OpenXmlAttribute>
               {
                  // row index
                  new OpenXmlAttribute("r", null, rowIdx.ToString())
               };

               // start row
               oxw.WriteStartElement(new Row(), oxa);

               for (int idx = 0; idx < dr.FieldCount; idx++)
                  OpenWorkbook.AddOpenXmlCell(oxw, GetStringOrNull(dr, idx));

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


      private static void AddOpenXmlHeaderCell(OpenXmlWriter writer, string val)
      {
         // this is the data type ("t"), with CellValues.String ("str")
         var oxa = new List<OpenXmlAttribute> { new OpenXmlAttribute("t", null, "str") };

         // start cell
         writer.WriteStartElement(new Cell() { StyleIndex = (UInt32)1 }, oxa);
         writer.WriteElement(new CellValue(val));

         // end cell
         writer.WriteEndElement();
      }


      private static void AddOpenXmlHeader(OpenXmlWriter oxw, string[] headerTitles)
      {
         var oxa = new List<OpenXmlAttribute> { new OpenXmlAttribute("r", null, "1") };

         // start row
         oxw.WriteStartElement(new Row(), oxa);

         foreach (string s in headerTitles)
            AddOpenXmlHeaderCell(oxw, s);

         // end row
         oxw.WriteEndElement();
      }


      private static void AddOpenXmlCell(OpenXmlWriter writer, string val)
      {
         var oxa = new List<OpenXmlAttribute> { new OpenXmlAttribute("t", null, "str") };

         // start cell
         writer.WriteStartElement(new Cell(), oxa);
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
