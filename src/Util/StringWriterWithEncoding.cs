using System;
using System.IO;
using System.Text;


namespace Util
{
   public class StringWriterWithEncoding : StringWriter
   {
      private readonly Encoding encode;

      public StringWriterWithEncoding(Encoding encoding)
      {
         encode = encoding;
      }


      public override Encoding Encoding
      {
         get { return encode; }
      }
   }
}
