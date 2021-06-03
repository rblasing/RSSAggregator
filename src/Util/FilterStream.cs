using System.IO;


namespace Util
{
   /// <summary>
   /// A stream which keeps an in-memory copy as it passes the bytes through
   /// </summary>
   [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
   public class FilterStream : Stream
   {
      private readonly Stream _innerStream;
      private readonly MemoryStream _copyStream;

      
      public FilterStream(Stream inner)
      {
         this._innerStream = inner;
         this._copyStream = new MemoryStream();
      }

      
      public string ReadStream()
      {
         lock (this._innerStream)
         {
            if (this._copyStream.Length <= 0L  ||
                !this._copyStream.CanRead  ||
                !this._copyStream.CanSeek)
            {
               return string.Empty;
            }

            long pos = this._copyStream.Position;
            this._copyStream.Position = 0L;

            try
            {
               return new StreamReader(this._copyStream).ReadToEnd();
            }
            finally
            {
               try
               {
                  this._copyStream.Position = pos;
               }
               catch
               {
               }
            }
         }
      }


      public override bool CanRead => this._innerStream.CanRead;

      public override bool CanSeek => this._innerStream.CanSeek;

      public override bool CanWrite => this._innerStream.CanWrite;

      public override void Flush() => this._innerStream.Flush();

      public override long Length => this._innerStream.Length;


      public override long Position
      {
         get => this._innerStream.Position;
         set => this._copyStream.Position = this._innerStream.Position = value;
      }


      public override int Read(byte[] buffer, int offset, int count)
      {
         return this._innerStream.Read(buffer, offset, count);
      }


      public override long Seek(long offset, SeekOrigin origin)
      {
         this._copyStream.Seek(offset, origin);

         return this._innerStream.Seek(offset, origin);
      }


      public override void SetLength(long value)
      {
         this._copyStream.SetLength(value);
         this._innerStream.SetLength(value);
      }


      public override void Write(byte[] buffer, int offset, int count)
      {
         this._copyStream.Write(buffer, offset, count);
         this._innerStream.Write(buffer, offset, count);
      }
   }
}