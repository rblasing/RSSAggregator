using System.IO;
using System.Text;


namespace Util
{
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   public class StringWriterWithEncoding : StringWriter
   {
      private readonly Encoding _encode;

      public StringWriterWithEncoding(Encoding encoding) => _encode = encoding;

      public override Encoding Encoding => _encode;
   }
}
